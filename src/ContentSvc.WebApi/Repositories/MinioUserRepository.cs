using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Context;
using ContentSvc.WebApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Repositories
{
    public class MinioUserRepository : RepositoryBase<MinioUser, string>, IMinioUserRepository
    {
        public MinioUserRepository(ContentSvcContext context) :
            base(context, ctx => (ctx as ContentSvcContext).MinioUsers)
        {
        }

        public async Task<ApiKey> GetApiKeyAsync(string key)
        {
            var context = _context as ContentSvcContext;
            var apiKey = await context.ApiKeys
                .Where(k => k.Key == key)
                .Include(k => k.MinioUser)
                .FirstOrDefaultAsync();
            return apiKey;
        }
    }
}
