using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContentSvc.Model.Entities
{
    public class Service : IEntity<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CreatorId { get; set; }
        [StringLength(16)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Desc { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid DeletedFlag { get; set; }

        public virtual ICollection<MinioUser> MinioUsers { get; set; }
    }
}
