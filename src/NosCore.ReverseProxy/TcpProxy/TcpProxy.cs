using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NosCore.Packets;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Login;
using NosCore.ReverseProxy.Configuration;
using NosCore.ReverseProxy.TcpClient;
using NosCore.ReverseProxy.TcpClientFactory;
using NosCore.Shared.Enumerations;

namespace NosCore.ReverseProxy.TcpProxy
{
    public class TcpProxy : IProxy
    {
        private readonly ILogger _logger;
        private readonly ReverseProxyConfiguration _configuration;
        private readonly byte[] _packet;
        private readonly ITcpClientFactory _tcpClientFactory;

        public TcpProxy(ILogger<TcpProxy> logger, ReverseProxyConfiguration configuration, ITcpClientFactory tcpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _tcpClientFactory = tcpClientFactory;
            var serializer = new Serializer(new[] { typeof(FailcPacket) });
            var packetString = serializer.Serialize(new FailcPacket
            {
                Type = LoginFailType.Maintenance
            });
            _packet = Encoding.UTF8.GetBytes($"{packetString} ");
            for (var i = 0; i < packetString.Length; i++)
            {
                _packet[i] = Convert.ToByte(_packet[i] + 15);
            }

            _packet[^1] = 25;
        }

        private async Task HandleClientAsync(CancellationToken stoppingToken, System.Net.Sockets.TcpClient remoteClient, ChannelConfiguration channelConfiguration)
        {
            _logger.LogInformation("Packet from {0} received", remoteClient.Client.RemoteEndPoint);
            var ip = (await Dns.GetHostAddressesAsync(channelConfiguration.RemoteHost)).First();
            remoteClient.NoDelay = true;
            remoteClient.ReceiveTimeout = _configuration.Timeout;
            using var client = _tcpClientFactory.CreateTcpClient();
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
                    await Task.Delay(1000);//this prevent sending close too fast
                    _logger.LogWarning("Maintenance Packet sent to {0}", remoteClient.Client.RemoteEndPoint);
                }
            }
        }

        private async Task StartChannelAsync(CancellationToken stoppingToken, ChannelConfiguration channelConfiguration)
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, channelConfiguration.LocalPort));
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            server.Start();

            _logger.LogInformation("proxy started {0} -> {1}", channelConfiguration.LocalPort, $"{channelConfiguration.RemoteHost}:{channelConfiguration.RemotePort}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var remoteClient = await server.AcceptTcpClientAsync();
                    await HandleClientAsync(stoppingToken, remoteClient, channelConfiguration);
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
