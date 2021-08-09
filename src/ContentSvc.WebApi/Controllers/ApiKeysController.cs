using ContentSvc.Model.Dto;
using ContentSvc.Model.Mapping;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositories.Interfaces;
using ContentSvc.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiKeysController : ControllerBase
    {
        private readonly ContentSvcContext _context;
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeysController(
            ContentSvcContext context,
            IApiKeyRepository apiKeyRepository)
        {
            _context = context;
            _apiKeyRepository = apiKeyRepository;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RevokeAsync(Guid id)
        {

            bool existing = await _apiKeyRepository.ExistsAsync(id);
            if (!existing)
            {
                return NoContent();
            }
            _apiKeyRepository.Remove(id);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
