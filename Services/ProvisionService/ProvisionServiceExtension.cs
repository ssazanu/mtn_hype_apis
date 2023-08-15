using helpers;
using helpers.Engine;
using helpers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ProvisionService.Features;
using ProvisionService.Providers;
using SharedLib;

namespace ProvisionService
{
    public static class ProvisionServiceExtension
    {
        public static IServiceCollection AddProvisionService(this IServiceCollection services)
        {
            services
                .AddSingleton<IDataProvider, DataProvider>()
                .AddSingleton<IIntegratorStorage, Providers.IntegratorStorage>()
                .AddSharedLibService()
                .AddSingleton<IBundleProvisionProvider, BundleProvisionProvider>()
                
                .AddSingleton<BaseServiceFeature, BundleProvision>()
                ;

            return services;
        }
    }
}
