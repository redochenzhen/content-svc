
using ContentSvc.WebApi.Repositories;
using ContentSvc.WebApi.Repositries.Interfaces;
using ContentSvc.WebApi.Services;
using ContentSvc.WebApi.Services.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExts
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services
                .AddScoped<IServiceRepository, ServiceRepository>()
                .AddScoped<IMinioUserRepository, MinioUserRepository>()
                .AddSingleton<IMcWrapper, McWrapper>();
        }
    }
}
