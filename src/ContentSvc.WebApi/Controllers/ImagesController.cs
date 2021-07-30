using ContentSvc.WebApi.ActionFilters;
using ContentSvc.WebApi.Helpers;
using ContentSvc.WebApi.Options;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
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

        // [HttpPost("upload")]
        // public async Task<IActionResult> UploadAsync(IFormFile file, string path)
        // {
        //     if (file == null) return BadRequest();
        //     if (path == null) return BadRequest();
        //     if (accessKey == null) return BadRequest();
        //     if (secretKey == null) return BadRequest();

        //     if (!file.ContentType.ToLower().StartsWith("image"))
        //     {
        //         return new UnsupportedMediaTypeResult();
        //     }

        //     var contentType = file.ContentType;
        //     var bucket = accessKey;
        //     var filePath = Path.Join(path, file.FileName).Replace("\\", "/");

        //     try
        //     {
        //         var minio = new MinioClient(_options.Endpoint);
        //         using (var stream = file.OpenReadStream())
        //         {
        //             await minio.PutObjectAsync(bucket, filePath, stream, file.Length, contentType);
        //         }
        //     }
        //     catch (MinioException ex)
        //     {
        //         throw ex;
        //     }
        //     var fullPath = Path.Join(bucket, filePath).Replace("\\", "/");
        //     return Created(new Uri(fullPath, UriKind.Relative), null);
        // }

        [FilesETagFilter("targetPath", "iconPath")]
        [ResponseCache(Duration = 30)]
        [HttpGet("watermark")]
        public async Task<IActionResult> GetWithWatermarkAsync(
            [FromQuery(Name = "target")] string targetPath,
            [FromQuery(Name = "icon")] string iconPath,
            string text,
            [FromQuery(Name = "w")] int width,
            [FromQuery(Name = "h")] int height,
            int x,
            int y,
            [FromQuery(Name = "fs")] int fontSize,
            double opacity = 1)
        {
            if (targetPath == null) return BadRequest();
            if (width < 0) return BadRequest();
            if (height < 0) return BadRequest();
            if (fontSize < 0) return BadRequest();
            if (opacity < 0) return BadRequest();

            var client = _clientFactory.CreateClient("minio");
            var response = await client.GetAsync(targetPath);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var targetStream = await response.Content.ReadAsStreamAsync();
            var iconStream = default(Stream);
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    iconStream = await client.GetStreamAsync(iconPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can not get watermark image. Error: {ex.Message}");
                }
            }

            using (var image = new MagickImage(targetStream))
            {
                if (iconStream != null)
                {
                    using (var markImage = new MagickImage(iconStream))
                    {
                        if (!markImage.HasAlpha)
                        {
                            markImage.Alpha(AlphaOption.Set);
                        }
                        if (width > 0 || height > 0)
                        {
                            markImage.Resize(width, height);
                        }
                        markImage.Evaluate(Channels.Opacity, EvaluateOperator.Multiply, opacity);
                        if (x < 0) x += (image.Width - markImage.Width);
                        if (y < 0) y += (image.Height - markImage.Height);
                        image.Composite(markImage, x, y, CompositeOperator.Over);
                    }
                }
                else if (text != null && width > 0 && height > 0)
                {
                    if (fontSize == 0) fontSize = height;
                    using (var textImage = new MagickImage(MagickColors.White, width, height))
                    {
                        var d = new Drawables()
                            .FillColor(MagickColors.Gray);
                        if (text.HasChinese())
                        {
                            d = d.TextEncoding(Encoding.UTF8)
                                .Font("Microsoft YaHei & Microsoft YaHei UI");
                            d = d.Text(width >> 1, (height + 0.68 * fontSize) / 2, text);
                        }
                        else
                        {
                            d = d.Text(width >> 1, (height + 0.72 * fontSize) / 2, text);
                        }
                        d = d.TextAntialias(true)
                            .FontPointSize(fontSize)
                            .TextAlignment(TextAlignment.Center);
                        textImage.Draw(d);
                        textImage.Evaluate(Channels.Opacity, EvaluateOperator.Multiply, opacity);
                        if (x < 0) x += (image.Width - textImage.Width);
                        if (y < 0) y += (image.Height - textImage.Height);
                        image.Composite(textImage, x, y, CompositeOperator.Over);
                    }
                }
                var type = response.Content.Headers.ContentType.MediaType;
                return File(image.ToByteArray(), type);
            }
        }
    }
}
