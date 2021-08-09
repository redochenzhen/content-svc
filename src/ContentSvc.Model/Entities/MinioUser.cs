using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ContentSvc.Model.Entities
{
    public class MinioUser : IEntity<string>
    {
        [StringLength(32)]
        [Column("access_key")]
        public string Id { get; set; }

        [NotMapped]
        public string AccessKey
        {
            get { return Id; }
            set { Id = value; }
        }

        public string SecretKey { get; set; }

        public int ServiceId { get; set; }

        public virtual Service Service { get; set; }

        public virtual ICollection<ApiKey> ApiKeys { get; set; }
    }
}
