using ConsoleRouting;
using Microsoft.Extensions.DependencyInjection;

namespace KyoFileSignerCLI
{
    public static class Program
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider GetService()
        {
            return _serviceProvider;
        }

        public static void Main(string[] args)
        {
            var servicesCollection = new ServiceCollection();
            var services = new DefaultServiceProviderFactory().CreateBuilder(servicesCollection);
            Configurator.Add(services);
            _serviceProvider = services.BuildServiceProvider();

            Routing.Handle(args);
        }
    }

}
