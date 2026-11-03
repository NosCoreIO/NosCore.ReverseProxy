using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.ReverseProxy.Configuration;
using NosCore.ReverseProxy.I18N;
using NosCore.ReverseProxy.TcpClient;
using NosCore.ReverseProxy.TcpClientFactory;
using NosCore.ReverseProxy.TcpProxy;
using NosCore.Packets.Enumerations;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.ReverseProxy.Tests
{
    [TestClass]
    public class TcpProxyTests
    {
        private static ReverseProxyConfiguration Configuration(ServerType type) => new()
        {
            Timeout = 5000,
            Channels = new List<ChannelConfiguration>
            {
                new() { ServerType = type, RemoteHost = "127.0.0.1", RemotePort = 1, LocalPort = 1 }
            }
        };

        private static TcpProxy.TcpProxy CreateProxy(ReverseProxyConfiguration configuration, ITcpClient server)
        {
            var factory = new Mock<ITcpClientFactory>();
            factory.Setup(f => f.CreateTcpClient()).Returns(server);
            var logLanguage = new Mock<ILogLanguageLocalizer<LogLanguageKey>>();
            logLanguage.Setup(l => l[It.IsAny<LogLanguageKey>()])
                .Returns((LogLanguageKey key) => new Microsoft.Extensions.Localization.LocalizedString(key.ToString(), key.ToString()));
            return new TcpProxy.TcpProxy(NullLogger<TcpProxy.TcpProxy>.Instance, configuration, factory.Object, logLanguage.Object);
        }

        private static async Task<(System.Net.Sockets.TcpClient Accepted, System.Net.Sockets.TcpClient Player)> ConnectPairAsync()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var player = new System.Net.Sockets.TcpClient();
            var connect = player.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
            var accepted = await listener.AcceptTcpClientAsync();
            await connect;
            listener.Stop();
            return (accepted, player);
        }

        private static Mock<ITcpClient> UnreachableServer()
        {
            var server = new Mock<ITcpClient>();
            server.Setup(s => s.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>())).ThrowsAsync(new SocketException());
            return server;
        }

        [TestMethod]
        public async Task AnUnreachableLoginServerAnswersWithTheMaintenancePacket()
        {
            var configuration = Configuration(ServerType.LoginServer);
            var proxy = CreateProxy(configuration, UnreachableServer().Object);
            var (accepted, player) = await ConnectPairAsync();
            using var _ = player;

            await proxy.HandleClientAsync(CancellationToken.None, accepted, configuration.Channels![0]);

            var buffer = new byte[64];
            var read = await player.GetStream().ReadAsync(buffer);
            var decoded = new char[read - 1];
            for (var i = 0; i < decoded.Length; i++)
            {
                decoded[i] = (char)(buffer[i] - 15);
            }

            Assert.AreEqual($"failc {(int)LoginFailType.Maintenance}", new string(decoded));
            Assert.AreEqual(25, buffer[read - 1]);
            Assert.AreEqual(0, await player.GetStream().ReadAsync(buffer));
        }

        [TestMethod]
        public async Task AnUnreachableWorldServerClosesThePlayerConnection()
        {
            var configuration = Configuration(ServerType.WorldServer);
            var proxy = CreateProxy(configuration, UnreachableServer().Object);
            var (accepted, player) = await ConnectPairAsync();
            using var _ = player;

            await proxy.HandleClientAsync(CancellationToken.None, accepted, configuration.Channels![0]);

            player.GetStream().ReadTimeout = 5000;
            Assert.AreEqual(-1, player.GetStream().ReadByte());
        }

        [TestMethod]
        public async Task TrafficIsForwardedBothWays()
        {
            var configuration = Configuration(ServerType.WorldServer);
            var (serverSide, serverPeer) = await ConnectPairAsync();
            using var __ = serverPeer;
            var server = new Mock<ITcpClient>();
            server.Setup(s => s.ConnectAsync(IPAddress.Loopback, 1)).Returns(Task.CompletedTask);
            server.Setup(s => s.GetStream()).Returns(serverSide.GetStream());
            var proxy = CreateProxy(configuration, server.Object);
            var (accepted, player) = await ConnectPairAsync();

            var handling = proxy.HandleClientAsync(CancellationToken.None, accepted, configuration.Channels![0]);

            await player.GetStream().WriteAsync(new byte[] { 1, 2, 3 });
            var fromPlayer = new byte[3];
            await serverPeer.GetStream().ReadExactlyAsync(fromPlayer);
            await serverPeer.GetStream().WriteAsync(new byte[] { 4, 5 });
            var fromServer = new byte[2];
            await player.GetStream().ReadExactlyAsync(fromServer);
            player.Close();
            await handling.WaitAsync(TimeSpan.FromSeconds(5));

            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, fromPlayer);
            CollectionAssert.AreEqual(new byte[] { 4, 5 }, fromServer);
            server.Verify(s => s.Close(), Times.Once);
        }

        [TestMethod]
        public void TheProxyResolvesWithTheSharedLogLanguage()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton(Configuration(ServerType.LoginServer))
                .AddSingleton<ITcpClientFactory, TcpClientFactory.TcpClientFactory>()
                .AddSingleton<IProxy, TcpProxy.TcpProxy>();
            services.AddI18NLogs();

            using var provider = services.BuildServiceProvider();

            Assert.IsInstanceOfType<TcpProxy.TcpProxy>(provider.GetRequiredService<IProxy>());
        }
    }
}
