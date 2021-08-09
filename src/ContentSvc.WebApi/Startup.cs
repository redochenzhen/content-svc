using ContentSvc.WebApi;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContentSvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ContentSvcContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddResponseCaching();

            services.AddControllers(options =>
            {
                var authenticatedUserPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(authenticatedUserPolicy));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });
            var secretBytes = Encoding.ASCII.GetBytes(Configuration["Token:Secret"]);
            services
                .AddAuthentication("Multiple")
                .AddPolicyScheme("Multiple", "Multiple Auth Scheme", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith(ApikeyDefaults.AuthenticationScheme))
                        {
                            return ApikeyDefaults.AuthenticationScheme;
                        }
                        return JwtBearerDefaults.AuthenticationScheme;
                    };
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddApikey();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Content Service API",
                    Version = "v1.x",
                });
                config.CustomSchemaIds(t => t.FullName);
                config.AddSecurityDefinition("Bearer",
                   new OpenApiSecurityScheme
                   {
                       Name = "Authorization",
                       Description = "JWT Authorization header using the Bearer scheme.",
                       Type = SecuritySchemeType.ApiKey,
                       Scheme = "Bearer",
                       In = ParameterLocation.Header
                   });
                config.AddSecurityDefinition("Apikey",
                   new OpenApiSecurityScheme
                   {
                       Name = "Authorization",
                       Description = "Authorization header using the Apikey scheme.",
                       Type = SecuritySchemeType.ApiKey,
                       Scheme = "Apikey",
                       In = ParameterLocation.Header
                   });
                config.AddSecurityRequirement(
                   new OpenApiSecurityRequirement
                   {
                       {
                           new OpenApiSecurityScheme
                           {
                               Reference = new OpenApiReference
                               {
                                   Id = "Bearer",
                                   Type = ReferenceType.SecurityScheme,
                               }
                           },
                           new string[]{ }
                       },
                       {
                           new OpenApiSecurityScheme
                           {
                               Reference = new OpenApiReference
                               {
                                   Id = "Apikey",
                                   Type = ReferenceType.SecurityScheme,
                               }
                           },
                           new string[]{ }
                       },
                   });
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddBuildInfo();

            services.Configure<MinioOptions>(Configuration.GetSection(MinioOptions.PREFIX));

            services.AddCors(options =>
            {
                var cors = Configuration.GetSection("Cors").Get<CorsConfig>();
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins(cors.Origins)
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Token-Expired")
                        .AllowCredentials());
            });

            // #if !DEBUG
            services.AddDiscovery(options =>
            {
                options.UseZooPicker();
            });
            // #endif
            var minio = Configuration.GetSection(MinioOptions.PREFIX).Get<MinioOptions>();
            services.AddHttpClient("minio", client =>
            {
                client.BaseAddress = minio.Secure ?
                    new Uri($"https://{minio.Endpoint}/") :
                    new Uri($"http://{minio.Endpoint}/");
            });

            services.AddCustomServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
#if !DEBUG
            //if (env.IsDevelopment())
#endif
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"ContentSvc.WebApi v1.x");
                options.InjectBuildInfo();
            });
            app.UseBuildInfo();
        }
    }

    class CorsConfig
    {
        public string[] Origins { get; set; }
    }
}
