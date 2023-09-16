using KyoFileSignerCLI.Common;
using KyoFileSignerCLI.Http;
using Microsoft.Extensions.DependencyInjection;

namespace KyoFileSignerCLI
{
    public static class Configurator
    {
        public static void Add(this IServiceCollection services)
        {
            services.AddSingleton<AdminRequest>();
            services.AddSingleton<AuthenticationRequest>();
            services.AddSingleton<SigningRequest>();
            services.AddHttpClient<AppClient>("AppClient", x => { x.BaseAddress = new Uri(ApiConstants.ApiBaseUrl); });
            services.AddSingleton<AppClientFactory>();
        }
    }
}