using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositories.Interfaces;

namespace ContentSvc.WebApi.Repositories
{
    public class ServiceRepository : RepositoryBase<Service,int>, IServiceRepository
    {
        public ServiceRepository(ContentSvcContext context) :
            base(context, ctx => (ctx as ContentSvcContext).Services)
        {
        }
    }
}
