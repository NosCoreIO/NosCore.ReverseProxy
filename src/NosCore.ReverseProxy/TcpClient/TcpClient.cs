//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NosCore.ReverseProxy.TcpClient
{
    public class TcpClient : ITcpClient
    {
        private readonly System.Net.Sockets.TcpClient _client;

        public Stream GetStream() => _client.GetStream();

        public TcpClient()
        {
            _client = new System.Net.Sockets.TcpClient();
        }

        public TcpClient(System.Net.Sockets.TcpClient client)
        {
            _client = client;
        }

        public Task ConnectAsync(IPAddress ip, int port)
        {
            return _client.ConnectAsync(ip, port);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}