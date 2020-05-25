//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.ReverseProxy
{
    public enum ServerType
    {
        WorldServer,
        LoginServer
    }
    public class ChannelConfiguration
    {
        public ServerType ServerType { get; set; }
        public string RemoteHost { get; set; }
        public ushort RemotePort { get; set; }
        public ushort LocalPort { get; set; }
    }
}