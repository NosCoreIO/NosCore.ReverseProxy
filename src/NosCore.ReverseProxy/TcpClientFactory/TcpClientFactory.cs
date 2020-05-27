using System;
using System.Collections.Generic;
using System.Text;
using NosCore.ReverseProxy.TcpClient;

namespace NosCore.ReverseProxy.TcpClientFactory
{
    public class TcpClientFactory : ITcpClientFactory
    {
        public ITcpClient CreateTcpClient()
        {
           return new TcpClient.TcpClient();
        }
    }
}
