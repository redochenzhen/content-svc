using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.ActionFilters
{
    public class FilesETagFilter : ActionFilterAttribute
    {
        private readonly string _pathArgName1;
        private readonly string _pathArgName2;

        public FilesETagFilter(string pathArgName1, string pathArgName2 = null)
        {
            _pathArgName1 = pathArgName1;
            _pathArgName2 = pathArgName2;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var args = context.ActionArguments;
            var httpCtx = context.HttpContext;
            args.TryGetValue(_pathArgName1, out var pathObj);
            string path1 = pathObj?.ToString() ?? throw new ArgumentNullException(_pathArgName1);
            args.TryGetValue(_pathArgName2, out var pathObj2);
            string path2 = pathObj2?.ToString();
            var inm = httpCtx.Request.Headers[HttpHeaders.If_None_Match];
            inm = new StringValues(inm.ToString().Split("."));

            var clientFactory = httpCtx.RequestServices.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
            var client = clientFactory?.CreateClient("minio");
            if (client == null)
            {
                await next();
                return;
            }

            var req1 = new HttpRequestMessage(HttpMethod.Get, path1);
            if (inm.Count > 0)
            {
                req1.Headers.Add(HttpHeaders.If_None_Match, inm[0]);
            }
            var res1 = await client.SendAsync(req1);
            if (!res1.IsSuccessStatusCode && res1.StatusCode != HttpStatusCode.NotModified)
            {
                context.Result = new StatusCodeResult((int)res1.StatusCode);
                return;
            }
            res1.Headers.TryGetValues(HttpHeaders.ETag, out var etags1);
            var etag1 = etags1?.FirstOrDefault();

            var etag2 = default(string);
            if (!string.IsNullOrEmpty(path2))
            {
                var req2 = new HttpRequestMessage(HttpMethod.Get, path2);
                if (inm.Count > 1)
                {
                    req2.Headers.Add(HttpHeaders.If_None_Match, inm[1]);
                }
                var res2 = await client.SendAsync(req2);
                if (res2.StatusCode == HttpStatusCode.NotModified || res2.IsSuccessStatusCode)
                {
                    res2.Headers.TryGetValues(HttpHeaders.ETag, out var etags2);
                    etag2 = etags2?.FirstOrDefault();
                }
            }

            var etag = Concat(etag1, etag2);
            if (inm.ToString() == etag)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
            }
            else
            {
                await next();
                httpCtx.Response.Headers.Add(HttpHeaders.ETag, etag);
            }
        }

        private static string Concat(string etag1, string etag2)
        {
            if (etag1 == null) return etag2;
            if (etag2 == null) return etag1;
            var sb = new StringBuilder("\"");
            return $"{etag1}.{etag2}";
        }
    }
}
