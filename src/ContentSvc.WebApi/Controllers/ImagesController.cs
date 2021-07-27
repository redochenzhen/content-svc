using ContentSvc.WebApi.Options;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly MinioOptions _options;
        private readonly IHttpClientFactory _clientFactory;

        public ImagesController(
            ILogger<ImagesController> logger,
            IOptions<MinioOptions> minioOptions,
            IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _options = minioOptions.Value;
            _clientFactory = clientFactory;
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
                var minio = new MinioClient(_options.Endpoint);
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

        [HttpGet("watermark")]
        public async Task<IActionResult> GetWithWatermark(
            [FromQuery(Name = "image")] string imagePath,
            [FromQuery(Name = "mark")] string watermarkPath,
            string text,
            [FromQuery(Name = "w")] int width,
            [FromQuery(Name = "h")] int height,
            int x,
            int y,
            double opacity = 1)
        {

            if (imagePath == null) return BadRequest();
            if (width < 0) return BadRequest();
            if (height < 0) return BadRequest();
            if (opacity < 0) return BadRequest();

            var client = _clientFactory.CreateClient("minio");
            var response = await client.GetAsync(imagePath);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var imageStream = await response.Content.ReadAsStreamAsync();
            var maskStream = default(Stream);
            if (!string.IsNullOrEmpty(watermarkPath))
            {
                try
                {
                    maskStream = await client.GetStreamAsync(watermarkPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can not get watermark image. Error: {ex.Message}");
                }
            }

            using (var image = new MagickImage(imageStream))
            {
                if (maskStream != null)
                {
                    using (var markImage = new MagickImage(maskStream))
                    {
                        if (!markImage.HasAlpha)
                        {
                            markImage.Alpha(AlphaOption.Set);
                        }
                        markImage.Resize(width, height);
                        markImage.Evaluate(Channels.Opacity, EvaluateOperator.Multiply, opacity);
                        if (x < 0) x += (image.Width - markImage.Width);
                        if (y < 0) y += (image.Height - markImage.Height);
                        image.Composite(markImage, x, y, CompositeOperator.Over);
                    }
                }
                else if (text != null && width > 0 && height > 0)
                {
                    using (var textImage = new MagickImage(MagickColors.White, width, height))
                    {
                        textImage.Draw(
                            new Drawables()
                            .FillColor(MagickColors.Gray)
                            .FontPointSize(height)
                            .Text(width >> 1, height * 0.9, text)
                            .TextAlignment(TextAlignment.Center));
                        textImage.Evaluate(Channels.Opacity, EvaluateOperator.Multiply, opacity);
                         if (x < 0) x += (image.Width - textImage.Width);
                        if (y < 0) y += (image.Height - textImage.Height);
                        image.Composite(textImage, x, y, CompositeOperator.Over);
                    }
                }
                return File(image.ToByteArray(), response.Content.Headers.ContentType.MediaType);
            }
        }
    }
}
