using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositories.Interfaces;
using System;

namespace ContentSvc.WebApi.Repositories
{
    public class ApiKeyRepository : RepositoryBase<ApiKey, Guid>, IApiKeyRepository
    {
        public ApiKeyRepository(ContentSvcContext context) :
            base(context, ctx => (ctx as ContentSvcContext).ApiKeys)
        {
        }
    }
}
