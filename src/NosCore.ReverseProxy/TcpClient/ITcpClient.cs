using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NosCore.ReverseProxy.TcpClient
{
    public interface ITcpClient : IDisposable
    {
        Task ConnectAsync(IPAddress ip, int port);
        Stream GetStream();
    }
}
