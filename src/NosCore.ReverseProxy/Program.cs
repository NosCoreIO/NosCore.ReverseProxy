using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.IO;

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
            var conf = new ConfigurationBuilder()
                .AddYamlFile("reverse-proxy.yml", false)
                .AddYamlFile("logger.yml")
                .Build();
            conf.Bind(configuration);
            Validator.ValidateObject(configuration, new ValidationContext(configuration), true);
            return Host.CreateDefaultBuilder(args)
                           .UseWindowsService()
                           .UseSystemd()
                           .ConfigureLogging(
                               loggingBuilder =>
                               {
                                   var logger = new LoggerConfiguration()
                                       .ReadFrom.Configuration(conf)
                                       .CreateLogger();
                                   loggingBuilder.ClearProviders();
                                   loggingBuilder.AddSerilog(logger, dispose: true);
                                   loggingBuilder.AddEventLog();
                               }
                           )
                           .ConfigureServices((hostContext, services) =>
                           {
                               services.AddSingleton(configuration);
                               services.AddSingleton(typeof(IProxy), typeof(TcpProxy));
                               services.AddHostedService<Worker>();
                           });
        }
    }

}
