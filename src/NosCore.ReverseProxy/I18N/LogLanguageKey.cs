//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Diagnostics.CodeAnalysis;

namespace NosCore.ReverseProxy.I18N
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LogLanguageKey
    {
        PACKET_RECEIVED,
        MAINTENANCE_PACKET_SENT,
        PROXY_STARTED,
        ERROR,
        DISCONNECTED
    }
}