using ContentSvc.Model.Dto;
using ContentSvc.Model.Mapping;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Minio;
using ContentSvc.WebApi.Repositries.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
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
        private readonly MinioOptions _minioOptions;

        public ServicesController(
            IOptions<MinioOptions> minioOptions,
            ContentSvcContext context,
            IServiceRepository serviceRepository,
            IMinioUserRepository minioUserRepository)
        {
            _minioOptions = minioOptions.Value;
            _context = context;
            _serviceRepository = serviceRepository;
            _minioUserRepository = minioUserRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateServiceAsync(Service serviceDto)
        {
            var now = DateTime.Now;
            var service = serviceDto.ToEntiry();
            service.CreatedDate = now;
            //service.CreatorId = 0;
            var minioUserDto = new MinioUser
            {
                AccessKey = service.Name,
                SecretKey = GenerateSecretKey(),
            };

            var minioUser = minioUserDto.ToEntiry();
            CreateMinioBucketAndUser(minioUser.AccessKey, minioUserDto.SecretKey);

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
            serviceDto.MinioUsers = new List<MinioUser> { minioUserDto };
            return Created(new Uri($"/api/services/{service.Id}", UriKind.Relative), serviceDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServicesAsync()
        {
            var services = await _context.Services
            .Include(s => s.MinioUsers)
            .ToListAsync();
            var serviceDtos = services.Select(svc => svc.ToDto()).ToList();
            return Ok(serviceDtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Service), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceDetailAsync(int id)
        {
            throw new NotImplementedException();
        }

        private string GenerateSecretKey()
        {
            var randomNumber = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private void CreateMinioBucketAndUser(string accessKey, string secretKey)
        {
            var policy = new Policy
            {
                Statement = new List<Statement>
                {
                    new Statement
                    {
                        Effect = Policy.Effect.ALLOW,
                        Action = new List<string>
                        {
                            Policy.Action.LIST_BUCKET,
                            Policy.Action.GET_BUCKET_LOCATION
                        },
                        Resource = new List<string>
                        {
                            Policy.Resource.Bucket(accessKey)
                        }
                    },
                    new Statement
                    {
                        Effect = Policy.Effect.ALLOW,
                        Action = new List<string>
                        {
                            Policy.Action.GET_OBJECT,
                            Policy.Action.PUT_OBJECT
                        },
                        Resource = new List<string>
                        {
                            Policy.Resource.Public(accessKey)
                        }
                    }
                }
            };

            string policyName = "cs_public_policy";
            string policyFileName = $"{policyName}.json";
            string json = JsonSerializer.Serialize(policy, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(policyFileName, json);
            var opt = _minioOptions;
            string minio = "mc_minio";
            var uri = opt.Secure ? $"https://{opt.Endpoint}" : $"http://{opt.Endpoint}";
            
            string mc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @".\mc.exe" : "./mc";
            RunProcess(mc, $"alias set {minio} {new Uri(uri)} {opt.AdminAccessKey} {opt.AdminSecretKey}");

            RunProcess(mc, $"mb {minio}/{accessKey}/public");

            RunProcess(mc, $"admin policy add {minio} {policyName} {policyFileName}");
            RunProcess(mc, $"admin user add {minio} {accessKey} {secretKey}");
            RunProcess(mc, $"admin policy set {minio} {policyName} user={accessKey}");

            RunProcess(mc, $"policy set download {minio}/{accessKey}/public");
        }

        static private int RunProcess(string fileName, string appParam)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(fileName, appParam)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();

                //while (!process.StandardOutput.EndOfStream)
                //{
                //    string line = process.StandardOutput.ReadLine();
                //    Console.WriteLine(line);
                //}

                //while (!process.StandardError.EndOfStream)
                //{
                //    string line = process.StandardError.ReadLine();
                //    Console.WriteLine(line);
                //}

                while (!process.HasExited)
                {
                    process.WaitForExit();
                }

                if (process.ExitCode == 0) return 0;
            }
            throw new Exception();
        }
    }
}
