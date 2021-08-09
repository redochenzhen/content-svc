using System;
using System.Collections.Generic;

namespace ContentSvc.Model.Dto
{
    public class MinioUser
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public List<ApiKey> ApiKeys { get; set; }
    }
}