using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositries;
using ContentSvc.WebApi.Repositries.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
