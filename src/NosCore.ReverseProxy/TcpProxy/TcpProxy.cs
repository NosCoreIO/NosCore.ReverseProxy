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
using NosCore.ReverseProxy.I18N;
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
            _packet = Encoding.Default.GetBytes($"{packetString} ");
            for (var i = 0; i < packetString.Length; i++)
            {
                _packet[i] = Convert.ToByte(_packet[i] + 15);
            }

            _packet[^1] = 25;
        }

        internal async Task HandleClientAsync(CancellationToken stoppingToken, System.Net.Sockets.TcpClient remoteClient, ChannelConfiguration channelConfiguration)
        {
            _logger.LogTrace(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.PACKET_RECEIVED), remoteClient.Client.RemoteEndPoint);
            var ip = (await Dns.GetHostAddressesAsync(channelConfiguration.RemoteHost ?? string.Empty)).First();
            remoteClient.NoDelay = true;
            remoteClient.ReceiveTimeout = _configuration.Timeout;
            using var client = _tcpClientFactory.CreateTcpClient();
            try
            {
                await client.ConnectAsync(ip, channelConfiguration.RemotePort);
                var serverStream = client.GetStream();
                var remoteStream = remoteClient.GetStream();

                await Task.WhenAny(remoteStream.CopyToAsync(serverStream, stoppingToken), serverStream.CopyToAsync(remoteStream, stoppingToken));
                _logger.LogTrace(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.PACKET_SENT), remoteClient.Client.RemoteEndPoint);
            }
            catch
            {
                if (channelConfiguration.ServerType == ServerType.LoginServer)
                {
                    await remoteClient.Client.SendAsync(_packet, SocketFlags.None);
                    await Task.Delay(1000, stoppingToken);//this prevent sending close too fast
                    _logger.LogWarning(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.MAINTENANCE_PACKET_SENT), remoteClient.Client.RemoteEndPoint);
                }
            }
        }

        private async Task StartChannelAsync(CancellationToken stoppingToken, ChannelConfiguration channelConfiguration)
        {
            var server = new TcpListener(IPAddress.Any, channelConfiguration.LocalPort);
            server.Start();

            _logger.LogInformation(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.PROXY_STARTED), channelConfiguration.LocalPort, $"{channelConfiguration.RemoteHost}:{channelConfiguration.RemotePort}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var remoteClient = await server.AcceptTcpClientAsync();
                    _ = HandleClientAsync(stoppingToken, remoteClient, channelConfiguration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.ERROR), ex);
                }

            }
        }

        public Task Start(CancellationToken stoppingToken)
        {
            return Task.WhenAll(_configuration.Channels!.Select(s => StartChannelAsync(stoppingToken, s)));
        }
    }
}
