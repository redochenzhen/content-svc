
using ContentSvc.WebApi.Repositories;
using ContentSvc.WebApi.Repositries.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExts
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services
                .AddScoped<IServiceRepository, ServiceRepository>()
                .AddScoped<IMinioUserRepository, MinioUserRepository>();
        }
    }
}
