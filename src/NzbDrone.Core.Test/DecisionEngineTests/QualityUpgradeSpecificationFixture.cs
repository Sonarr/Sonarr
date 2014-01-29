using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class QualityUpgradeSpecificationFixture : CoreTest<QualityUpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.SDTV, false, Quality.SDTV, true, Quality.SDTV, true },
            new object[] { Quality.WEBDL720p, false, Quality.WEBDL720p, true, Quality.WEBDL720p, true },
            new object[] { Quality.SDTV, false, Quality.SDTV, false, Quality.SDTV, false },
            new object[] { Quality.WEBDL720p, false, Quality.HDTV720p, true, Quality.Bluray720p, false },
            new object[] { Quality.WEBDL720p, false, Quality.HDTV720p, true, Quality.WEBDL720p, false },
            new object[] { Quality.WEBDL720p, false, Quality.WEBDL720p, false, Quality.WEBDL720p, false },
            new object[] { Quality.SDTV, false, Quality.SDTV, true, Quality.SDTV, true },
            new object[] { Quality.WEBDL1080p, false, Quality.WEBDL1080p, false, Quality.WEBDL1080p, false }
        };
        
        [SetUp]
        public void Setup()
        {

        }

        private void GivenAutoDownloadPropers(bool autoDownloadPropers)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoDownloadPropers)
                  .Returns(autoDownloadPropers);
        }

        [Test, TestCaseSource("IsUpgradeTestCases")]
        public void IsUpgradeTest(Quality current, bool currentProper, Quality newQuality, bool newProper, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(true);

            var qualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(qualityProfile, new QualityModel(current, currentProper), new QualityModel(newQuality, newProper))
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_false()
        {
            GivenAutoDownloadPropers(false);

            var qualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(qualityProfile, new QualityModel(Quality.DVD, true), new QualityModel(Quality.DVD, false))
                    .Should().BeFalse();
        }
    }
}