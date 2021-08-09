using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ContentSvc.Model.Entities
{
    public class ApiKey : IEntity<Guid>
    {
        public Guid Id { get; set; }
        [StringLength(28)]
        public string Key { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public ApiRole Role { get; set; }
        [StringLength(32)]
        [Required]
        public string MinioUserId { get; set; }
        [StringLength(64)]
        public string Remarks { get; set; }

        public virtual MinioUser MinioUser { get; set; }
    }
}
