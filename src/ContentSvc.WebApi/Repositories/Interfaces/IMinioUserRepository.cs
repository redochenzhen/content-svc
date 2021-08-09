using ContentSvc.Model.Entities;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Repositories.Interfaces
{
    public interface IMinioUserRepository : IRepository<MinioUser, string>
    {
        Task<ApiKey> GetApiKeyAsync(string key);
    }
}
