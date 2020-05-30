//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NosCore.Shared.Configuration;

namespace NosCore.ReverseProxy.Configuration
{
    public class ReverseProxyConfiguration : LanguageConfiguration
    {
        [Required]
        public List<ChannelConfiguration>? Channels { get; set; }

        public ushort Timeout { get; set; }
    }
}