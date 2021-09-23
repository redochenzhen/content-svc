using System.Collections.Generic;
using System.Linq;

namespace ContentSvc.WebApi.Options
{
    public class MinioOptions
    {
        public const string PREFIX = "MinIO";

        public string Alias { get; set; } = "minio-contentservice";
        public string Endpoint => Endpoints?.FirstOrDefault();
        public List<string> Endpoints { get; set; }
        public bool Secure { get; set; }
        public string AdminAccessKey { get; set; }
        public string AdminSecretKey { get; set; }

        public string PublicBaseUrl { get; set; }
        public string ConsoleUrl { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
