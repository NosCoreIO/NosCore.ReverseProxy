using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosCore.ReverseProxy.Configuration;
using NosCore.ReverseProxy.TcpClientFactory;
using NosCore.ReverseProxy.TcpProxy;
using Serilog;
using NosCore.Shared.Configuration;

namespace NosCore.ReverseProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ReverseProxyConfiguration();
            ConfiguratorBuilder.InitializeConfiguration(args, new[] { "logger.yml", "reverse-proxy.yml" }, configuration);
            return Host.CreateDefaultBuilder(args)
                           .UseWindowsService()
                           .UseSystemd()
                           .ConfigureLogging(
                               loggingBuilder =>
                               {
                                   loggingBuilder.ClearProviders();
                                   loggingBuilder.AddSerilog(dispose: true);
                               }
                           )
                           .ConfigureServices((hostContext, services) =>
                           {
                               services.AddSingleton(configuration);
                               services.AddSingleton(typeof(ITcpClientFactory), typeof(TcpClientFactory.TcpClientFactory));
                               services.AddSingleton(typeof(IProxy), typeof(TcpProxy.TcpProxy));
                               services.AddHostedService<Worker>();
                           });
        }
    }

}
