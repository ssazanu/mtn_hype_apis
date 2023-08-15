using helpers;
using helpers.Engine;
using helpers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SharedLib;

namespace UserService
{
    public static class ProvisionServiceExtension
    {
        public static IServiceCollection AddProvisionService(this IServiceCollection services)
        {
            //services
                
            //    .AddSingleton<BaseServiceFeature, >()
            //    ;

            return services;
        }
    }
}
