using ContentSvc.WebApi.Options;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly MinioOptions _options;

        public ImagesController(IOptions<MinioOptions> minioOptions)
        {
            _options = minioOptions.Value;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file, string path, string accessKey, string secretKey)
        {
            if (file == null) return BadRequest();
            if (path == null) return BadRequest();
            if (accessKey == null) return BadRequest();
            if (secretKey == null) return BadRequest();

            if (!file.ContentType.ToLower().StartsWith("image"))
            {
                return new UnsupportedMediaTypeResult();
            }

            var contentType = file.ContentType;
            var bucket = accessKey;
            var filePath = Path.Join(path, file.FileName).Replace("\\", "/");

            try
            {
                var minio = new MinioClient(_options.Endpoint, accessKey, secretKey);
                using (var stream = file.OpenReadStream())
                {
                    await minio.PutObjectAsync(bucket, filePath, stream, file.Length, contentType);
                }
            }
            catch (MinioException ex)
            {
                throw ex;
            }
            var fullPath = Path.Join(bucket, filePath).Replace("\\", "/");
            return Created(new Uri(fullPath, UriKind.Relative), null);
        }
    }
}
