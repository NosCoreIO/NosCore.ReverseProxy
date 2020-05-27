using NosCore.ReverseProxy.TcpClient;

namespace NosCore.ReverseProxy.TcpClientFactory
{
    public interface ITcpClientFactory
    {
        ITcpClient CreateTcpClient();
    }
}
