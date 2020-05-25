//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.ReverseProxy
{
    public class ReverseProxyConfiguration
    {
        public List<ChannelConfiguration> Channels { get; set; }

        public ushort Timeout { get; set; }
    }
}