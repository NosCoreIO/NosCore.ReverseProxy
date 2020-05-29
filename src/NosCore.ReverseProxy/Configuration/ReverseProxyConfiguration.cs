//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NosCore.ReverseProxy.Configuration
{
    public class ReverseProxyConfiguration
    {
        [Required]
        public List<ChannelConfiguration>? Channels { get; set; }

        public ushort Timeout { get; set; }
    }
}