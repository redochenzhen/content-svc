using ContentSvc.WebApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Security
{
    public class ApikeyAuthenticationHandler : AuthenticationHandler<ApikeyOptions>
    {
        private readonly IMinioUserRepository _minioUserRepository;

        public ApikeyAuthenticationHandler(
            IOptionsMonitor<ApikeyOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMinioUserRepository minioUserRepository)
            : base(options, logger, encoder, clock)
        {
            _minioUserRepository = minioUserRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Context.Request.Headers["Authorization"].FirstOrDefault();
            var key = authHeader.Substring(ApikeyDefaults.AuthenticationScheme.Length + 1);
            var apiKey = await _minioUserRepository.GetApiKeyAsync(key);
            if (apiKey == null || apiKey.MinioUser == null)
            {
                return AuthenticateResult.Fail("Bad api key.");
            }
            if (apiKey.ExpiredAt != null && apiKey.ExpiredAt < DateTime.Now)
            {
                return AuthenticateResult.Fail("Api key is expired.");
            }
            var minioUser = apiKey.MinioUser;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "ApiUser"),
                new Claim("access-key", minioUser.AccessKey),
                new Claim("secret-key", minioUser.SecretKey)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new GenericPrincipal(identity, new[] { apiKey.Role.ToString() });
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
