using ContentSvc.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DTO = ContentSvc.Model.Dto;

namespace ContentSvc.Model.Mapping
{
    public static class ServiceMapping
    {
        public static Service ToEntiry(this DTO.Service service)
        {
            return new Service
            {
                Id = service.Id,
                Name = service.Name,
                Desc = service.Desc,
                CreatedDate = service.CreatedDate,
            };
        }

        public static DTO.Service ToDto(this Service service)
        {
            return new DTO.Service
            {
                Id = service.Id,
                Name = service.Name,
                Desc = service.Desc,
                CreatedDate = service.CreatedDate,
                CreatorId = service.CreatorId,
                MinioUsers = service.MinioUsers?
                    .Select(u => u.ToDto())
                    .ToList()
            };
        }
    }
}
