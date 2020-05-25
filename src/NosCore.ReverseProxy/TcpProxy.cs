using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NosCore.Packets;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Login;

namespace NosCore.ReverseProxy
{
    public class TcpProxy : IProxy
    {
        private readonly ILogger _logger;
        private readonly ReverseProxyConfiguration _configuration;

        public TcpProxy(ILogger<TcpProxy> logger, ReverseProxyConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Start(CancellationToken stoppingToken)
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, _configuration.LocalPort));
            var ip = (await Dns.GetHostAddressesAsync(_configuration.RemoteHost)).First();
            var serializer = new Serializer(new[] { typeof(FailcPacket) });
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            server.Start();

            _logger.LogInformation("proxy started {0} -> {1}", _configuration.LocalPort, $"{_configuration.RemoteHost}:{_configuration.RemotePort}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var remoteClient = await server.AcceptTcpClientAsync();
                    _logger.LogInformation("Packet from {0} received", remoteClient.Client.RemoteEndPoint);
                    remoteClient.NoDelay = true;
                    remoteClient.ReceiveTimeout = _configuration.Timeout;
                    using (remoteClient)
                    using (var client = new TcpClient())
                    {
                        try
                        {
                            await client.ConnectAsync(ip, _configuration.RemotePort);
                            var serverStream = client.GetStream();
                            var remoteStream = remoteClient.GetStream();
                            if (remoteStream.Length > 0)
                            {
                                await Task.WhenAny(remoteStream.CopyToAsync(serverStream, stoppingToken), serverStream.CopyToAsync(remoteStream, stoppingToken));
                                _logger.LogInformation("Packet received from {0}", remoteClient.Client.RemoteEndPoint);
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        catch
                        {
                            var packetString = serializer.Serialize(new FailcPacket
                            {
                                Type = LoginFailType.Maintenance
                            });
                            var tmp = Encoding.ASCII.GetBytes($"{packetString} ");
                            for (var i = 0; i < packetString.Length; i++)
                            {
                                tmp[i] = Convert.ToByte(tmp[i] + 15);
                            }

                            tmp[^1] = 25;
                            await remoteClient.Client.SendAsync(tmp.Length == 0 ? new byte[] { 0xFF } : tmp, SocketFlags.None);

                            _logger.LogWarning("Maintenance Packet sent to {0}", remoteClient.Client.RemoteEndPoint);
                            remoteClient.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occurred", ex);
                }

            }
        }
    }
}
