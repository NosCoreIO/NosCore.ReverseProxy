using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.ReverseProxy.I18N;
using NosCore.ReverseProxy.Resource;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.ReverseProxy.Tests
{
    [TestClass]
    public class LogLanguageTests
    {
        private readonly LogLanguageLocalizer<LogLanguageKey, LocalizedResources> _logLanguage =
            new(new StringLocalizer<LocalizedResources>(
                new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()), new LoggerFactory())));

        [DataTestMethod]
        [DataRow(RegionType.EN)]
        [DataRow(RegionType.CS)]
        [DataRow(RegionType.DE)]
        [DataRow(RegionType.ES)]
        [DataRow(RegionType.FR)]
        [DataRow(RegionType.IT)]
        [DataRow(RegionType.PL)]
        [DataRow(RegionType.TR)]
        [DataRow(RegionType.RU)]
        public void EveryKeyHasATranslation(RegionType region)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(region.ToString());

            var missing = I18NTestHelpers.GetKeysWithMissingTranslations(_logLanguage).ToList();

            Assert.AreEqual(0, missing.Count, string.Join(", ", missing));
        }

        [DataTestMethod]
        [DataRow(RegionType.EN)]
        [DataRow(RegionType.FR)]
        public void EveryTranslationHasAKey(RegionType region)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(region.ToString());

            var useless = I18NTestHelpers.GetUselessTranslations(_logLanguage,
                Enum.GetNames<LogLanguageKey>().ToList()).ToList();

            Assert.AreEqual(0, useless.Count, string.Join(", ", useless));
        }

        [TestMethod]
        public void MessagesAreReadFromTheResources()
        {
            CultureInfo.CurrentUICulture = new CultureInfo(RegionType.EN.ToString());

            Assert.AreEqual("Packet received from {0}", _logLanguage[LogLanguageKey.PACKET_RECEIVED].Value);
        }
    }
}
