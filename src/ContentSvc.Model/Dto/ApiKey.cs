using System;
using System.Collections.Generic;
using System.Text;

namespace ContentSvc.Model.Dto
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string DesensitizedKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public ApiRole Role { get; set; }
        public string Remarks { get; set; }
    }
}
