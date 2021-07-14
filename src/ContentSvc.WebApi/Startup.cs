using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Minio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            services.AddControllers();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Content Service API",
                    Version = "v1.x",
                });
                config.CustomSchemaIds(t => t.FullName);
                //config.AddSecurityDefinition("Bearer",
                //    new OpenApiSecurityScheme
                //    {
                //        Name = "Authorization",
                //        Description = "JWT Authorization header using the Bearer scheme.",
                //        Type = SecuritySchemeType.ApiKey,
                //        Scheme = "Bearer",
                //        In = ParameterLocation.Header
                //    });
                //config.AddSecurityRequirement(
                //    new OpenApiSecurityRequirement
                //    {
                //        {
                //            new OpenApiSecurityScheme
                //            {
                //                Reference = new OpenApiReference
                //                {
                //                    Id = "Bearer",
                //                    Type = ReferenceType.SecurityScheme,
                //                }
                //            },
                //            new string[]{ }
                //        }
                //    });
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

            services.AddDiscovery(options=>
            {
                options.UseZooPicker();
            });

            services.AddCustomServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseRouting();

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
