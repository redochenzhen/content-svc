using ContentSvc.Model.Dto;
using ContentSvc.Model.Mapping;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositories.Interfaces;
using ContentSvc.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MinioUsersController : ControllerBase
    {
        private readonly ContentSvcContext _context;
        private readonly IMinioUserRepository _minioUserRepository;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly ITokenService _tokenService;

        public MinioUsersController(
            ContentSvcContext context,
            IMinioUserRepository minioUserRepository,
            IApiKeyRepository apiKeyRepository,
            ITokenService tokenService)
        {
            _context = context;
            _minioUserRepository = minioUserRepository;
            _apiKeyRepository = apiKeyRepository;
            _tokenService = tokenService;
        }

        [HttpPost("{id}/apikeys")]
        public async Task<IActionResult> CreateApiKeyAsync(string id, ApiKey apiKeyDto)
        {
            bool existing = await _minioUserRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var apiKey = apiKeyDto.ToEntity();
            apiKey.Id = Guid.NewGuid();
            apiKey.MinioUserId = id;
            apiKey.CreatedAt = DateTime.Now;
            apiKey.Key = _tokenService.GenerateRandomToken(28);
            _apiKeyRepository.Add(apiKey);
            await _context.SaveChangesAsync();
            apiKeyDto = apiKey.ToDto();
            apiKeyDto.Key = apiKey.Key;
            return Created(new Uri($"/api/miniousers/{id}/apikeys/{apiKey.Id}", UriKind.Relative), apiKeyDto);
        }
    }
}
