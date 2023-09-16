using Microsoft.Extensions.DependencyInjection;

namespace KyoFileSignerCLI.Http
{
    public class AppClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public AppClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public AppClient Create()
        {
            return _serviceProvider.GetRequiredService<AppClient>();
        }
    }
}