using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace ContentSvc.WebApi.ActionFilters
{
    public class FileETagFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is FileContentResult content)
            {
                var httpCtx = context.HttpContext;
                if (httpCtx.Request.Method == HttpMethod.Get.Method)
                {
                    if (httpCtx.Response.StatusCode == (int)HttpStatusCode.OK)
                    {
                        var inm = httpCtx.Request.Headers[HttpHeaders.If_None_Match];
                        var etag = ETagGenerator.GenerateETag(content.FileContents);
                        if (inm.ToString() == etag)
                        {
                            context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
                        }
                        else
                        {
                            httpCtx.Response.Headers.Add(HttpHeaders.ETag, new[] { etag });
                        }
                    }
                }
            }
        }
    }

    public static class ETagGenerator
    {
        public static string GenerateETag(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                var sb = new StringBuilder("\"");
                sb.Append(BitConverter.ToString(hash));
                sb.Replace("-", string.Empty);
                sb.Append("\"");
                return sb.ToString().ToLower();
            }
        }
    }

    public static class HttpHeaders
    {
        public const string If_None_Match = "If-None-Match";

        public const string ETag = "ETag";
    }
}