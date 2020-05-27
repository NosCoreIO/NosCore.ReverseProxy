//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace NosCore.ReverseProxy.TcpProxy
{
    public interface IProxy
    {
        Task Start(CancellationToken stoppingToken);
    }
}