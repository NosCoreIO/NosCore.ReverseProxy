using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NosCore.ReverseProxy.TcpProxy;
using NosCore.Shared.I18N;

namespace NosCore.ReverseProxy
{
    public class Worker : BackgroundService
    {
        private const string ConsoleText = "REVERSE PROXY - NosCoreIO";
        private readonly IProxy _proxy;

        public Worker(IProxy proxy)
        {
            _proxy = proxy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Logger.PrintHeader(ConsoleText);
            }

            await _proxy.Start(stoppingToken);
        }
    }
}
