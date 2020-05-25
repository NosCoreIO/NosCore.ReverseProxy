using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

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
                .AddYamlFile("reverse-proxy.yml", false).Build();
            conf.Bind(configuration);
            Validator.ValidateObject(configuration, new ValidationContext(configuration), true);
            return Host.CreateDefaultBuilder(args)
                           .ConfigureLogging(
                               loggingBuilder =>
                               {
                                   var build = new ConfigurationBuilder()
                                       .AddYamlFile("logger.yml")
                                       .Build();
                                   var logger = new LoggerConfiguration()
                                       .ReadFrom.Configuration(build)
                                       .CreateLogger();
                                   loggingBuilder.ClearProviders();
                                   loggingBuilder.AddSerilog(logger, dispose: true);
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
