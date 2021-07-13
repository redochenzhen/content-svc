using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Minio
{
    public class MinioOptions
    {
        public const string PREFIX = "MinIO";

        public string Endpoint { get; set; }
        public bool Secure { get; set; }
        public string AdminAccessKey { get; set; }
        public string AdminSecretKey { get; set; }
    }
}
