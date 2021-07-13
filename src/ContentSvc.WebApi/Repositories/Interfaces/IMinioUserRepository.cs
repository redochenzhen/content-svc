using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Repositries.Interfaces
{
    public interface IMinioUserRepository : IRepository<MinioUser, string>
    {
    }
}
