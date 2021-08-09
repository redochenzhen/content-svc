using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ContentSvc.WebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IWebHostEnvironment _environment;

        public TokenService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _environment = env;
        }

        public string GenerateRandomToken(int length)
        {
            int byteSize = length * 3 / 4;
            var randomNumber = new byte[byteSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public int GetMemberId(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var sub = user.FindFirst(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(sub.Value, out var id))
            {
                throw new Exception("invalid claim: sub");
            }
            return id;
        }

        public MinioUser GetMinioUser(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var accessKey = user.FindFirstValue("access-key");
            var secretKey = user.FindFirstValue("secret-key");
            return new MinioUser
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };
        }
    }
}