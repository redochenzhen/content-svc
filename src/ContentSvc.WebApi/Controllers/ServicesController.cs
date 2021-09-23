using ContentSvc.Model.Dto;
using ContentSvc.Model.Mapping;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Options;
using ContentSvc.WebApi.Repositories.Interfaces;
using ContentSvc.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ContentSvcContext _context;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMinioUserRepository _minioUserRepository;
        private readonly MinioOptions _options;
        private readonly ITokenService _tokenService;
        private readonly IMcWrapper _mc;

        public ServicesController(
            IOptions<MinioOptions> minioOptions,
            ContentSvcContext context,
            IServiceRepository serviceRepository,
            IMinioUserRepository minioUserRepository,
            ITokenService tokenService,
            IMcWrapper mc)
        {
            _options = minioOptions.Value;
            _context = context;
            _serviceRepository = serviceRepository;
            _minioUserRepository = minioUserRepository;
            _tokenService = tokenService;
            _mc = mc;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateServiceAsync(Service serviceDto)
        {
            var myId = _tokenService.GetMemberId(User);
            var now = DateTime.Now;
            var service = serviceDto.ToEntiry();
            service.CreatedDate = now;
            service.CreatorId = myId;
            var minioUserDto = new MinioUser
            {
                AccessKey = service.Name,
                SecretKey = _tokenService.GenerateRandomToken(20)
            };
            var minioUser = minioUserDto.ToEntiry();
            string accessKey = minioUser.AccessKey;
            string secretKey = minioUser.SecretKey;
            string bucket = accessKey;
            _mc.SetAlias();
            _mc.MakePublicBucket(bucket);
            string policy = _mc.AddBucketPolicy(bucket);
            _mc.AddUser(accessKey, secretKey);
            _mc.SetPolicy(policy, accessKey);
            _mc.SetPublicDownload(bucket);

            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _serviceRepository.Add(service);
                    await _context.SaveChangesAsync();
                    minioUser.ServiceId = service.Id;
                    _minioUserRepository.Add(minioUser);
                    await _context.SaveChangesAsync();
                    await trans.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    await trans.RollbackAsync();
                    return Conflict();
                }
            }
            serviceDto.Id = service.Id;
            serviceDto.CreatorId = myId;
            serviceDto.MinioUsers = new List<MinioUser> { minioUserDto };
            serviceDto.PublicBaseUrl = _options.PublicBaseUrl;
            serviceDto.Endpoints = _options.Endpoints ?? new List<string> { _options.Endpoint };
            serviceDto.ConsoleUrl = _options.ConsoleUrl;
            serviceDto.ApiBaseUrl = _options.ApiBaseUrl;
            return Created(new Uri($"/api/services/{service.Id}", UriKind.Relative), serviceDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServicesAsync()
        {
            var services = await _context.Services
            .Include(s => s.MinioUsers)
            .ToListAsync();
            var serviceDtos = services
                .Select(svc =>
                {
                    var dto = svc.ToDto();
                    dto.PublicBaseUrl = _options.PublicBaseUrl;
                    dto.ConsoleUrl = _options.ConsoleUrl;
                    dto.Endpoints = _options.Endpoints ?? new List<string> { _options.Endpoint };
                    dto.ApiBaseUrl = _options.ApiBaseUrl;
                    return dto;
                })
                .ToList();
            return Ok(serviceDtos);
        }

        //[HttpGet("{id}")]
        //[ProducesResponseType(typeof(Service), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> GetServiceDetailAsync(int id)
        //{
        //    throw new NotImplementedException();
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveServiceAsync(int id)
        {
            var myId = _tokenService.GetMemberId(User);
            var service = await _serviceRepository.GetAsync(id);
            if (service != null)
            {
                if (service.CreatorId != myId)
                {
                    return Forbid();
                }
                _serviceRepository.Remove(service);
                await _context.SaveChangesAsync();
                string accessKey = service.Name;
                string bucket = accessKey;
                _mc.RemoveBucketPolicy(bucket);
                _mc.RemoveUser(accessKey);
                _mc.ForceRemoveBucket(bucket);
            }
            return Ok();
        }

        [HttpGet("{id}/apikeys")]
        public async Task<IActionResult> GetApiKeysAsync(int id)
        {
            bool existing = await _serviceRepository.ExistsAsync(id);
            if (!existing)
            {
                return NotFound();
            }
            var apiKeys = await _context.Services
                .Include(s => s.MinioUsers)
                .ThenInclude(u => u.ApiKeys)
                .Where(s => s.Id == id)
                .SelectMany(s => s.MinioUsers.SelectMany(u => u.ApiKeys))
                .ToListAsync();
            var apiKeyDtos = apiKeys.Select(k => k.ToDto()).ToList();
            return Ok(apiKeyDtos);
        }
    }
}
