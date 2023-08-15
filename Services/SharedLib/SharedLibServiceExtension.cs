using Microsoft.Extensions.DependencyInjection;
using SharedLib.Interfaces;
using SharedLib.Providers.CategoryProvider;
using SharedLib.Providers.ProvisionProvider;
using ucip_lib_v5;

namespace SharedLib
{
    public static class SharedLibServiceExtension
    {
        public static IServiceCollection AddSharedLibService(this IServiceCollection services)
        {
            services
                .AddSingleton<UcipMocks>()
                .AddSingleton<IUcipCommand, UcipCommand>()
                .AddSingleton<IMessageBuilder, MessageBuilder>()
                .AddSingleton<IAirServerManager, AirServerManager>()
                .AddSingleton<IExternalDataManager, ExternalDataManager>()
                .AddSingleton<ICommandResponseProcessor, CommandResponseProcessor>()
                .AddSingleton<IUcipV4ResourceCommandHelper, UcipV4ResourceCommandHelper>()
                .AddSingleton<IUcipV4CommandBuilder, UcipV4CommandBuilder>()
                .AddSingleton<IUcipHelper, UcipHelper>()
                .AddSingleton<BaseConfigurationProvider, BundleConfigurationProvider>()
                .AddSingleton<IChargingProvider, ChargingProvider>()
            ;

            return services;
        }
    }
}
