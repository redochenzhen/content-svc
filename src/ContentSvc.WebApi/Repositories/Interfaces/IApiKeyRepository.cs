using ContentSvc.Model.Entities;
using System;

namespace ContentSvc.WebApi.Repositories.Interfaces
{
    public interface IApiKeyRepository : IRepository<ApiKey, Guid>
    {
    }
}
