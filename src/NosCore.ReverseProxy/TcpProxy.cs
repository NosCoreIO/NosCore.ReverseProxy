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
        private readonly byte[] _packet;
        public TcpProxy(ILogger<TcpProxy> logger, ReverseProxyConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var serializer = new Serializer(new[] { typeof(FailcPacket) });
            var packetString = serializer.Serialize(new FailcPacket
            {
                Type = LoginFailType.Maintenance
            });
            _packet = Encoding.ASCII.GetBytes($"{packetString} ");
            for (var i = 0; i < packetString.Length; i++)
            {
                _packet[i] = Convert.ToByte(_packet[i] + 15);
            }

            _packet[^1] = 25;
        }

        private async Task StartChannelAsync(CancellationToken stoppingToken, ChannelConfiguration channelConfiguration)
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, channelConfiguration.LocalPort));
            var ip = (await Dns.GetHostAddressesAsync(channelConfiguration.RemoteHost)).First();
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            server.Start();

            _logger.LogInformation("proxy started {0} -> {1}", channelConfiguration.LocalPort, $"{channelConfiguration.RemoteHost}:{channelConfiguration.RemotePort}");
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
                            await client.ConnectAsync(ip, channelConfiguration.RemotePort);
                            var serverStream = client.GetStream();
                            var remoteStream = remoteClient.GetStream();

                            await Task.WhenAny(remoteStream.CopyToAsync(serverStream, stoppingToken), serverStream.CopyToAsync(remoteStream, stoppingToken));
                            _logger.LogInformation("Packet received from {0}", remoteClient.Client.RemoteEndPoint);
                        }
                        catch
                        {
                            if (channelConfiguration.ServerType == ServerType.LoginServer)
                            {
                                await remoteClient.Client.SendAsync(_packet, SocketFlags.None);
                                _logger.LogWarning("Maintenance Packet sent to {0}", remoteClient.Client.RemoteEndPoint);
                            }
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

        public Task Start(CancellationToken stoppingToken)
        {
            return Task.WhenAll(_configuration.Channels.Select(s => StartChannelAsync(stoppingToken, s)));
        }
    }
}
