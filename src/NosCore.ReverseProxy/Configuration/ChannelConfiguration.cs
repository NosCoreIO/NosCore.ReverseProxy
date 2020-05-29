//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.ComponentModel.DataAnnotations;
using NosCore.Shared.Enumerations;

namespace NosCore.ReverseProxy.Configuration
{
    public class ChannelConfiguration
    {
        [Required]
        public ServerType? ServerType { get; set; }
        [Required]
        public string? RemoteHost { get; set; }
        [Range(1, ushort.MaxValue)]
        public ushort RemotePort { get; set; }
        [Range(1, ushort.MaxValue)]
        public ushort LocalPort { get; set; }
    }
}