using System;
using System.Collections.Generic;

namespace ContentSvc.Model.Dto
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatorId { get; set; }
        public List<MinioUser> MinioUsers { get; set; }

        public string PublicBaseUrl { get; set; }
        public string ConsoleUrl { get; set; }
        public string Endpoint { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
