using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.SDTV, 1, Quality.SDTV, 2, Quality.SDTV, true },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 2, Quality.WEBDL720p, true },
            new object[] { Quality.SDTV, 1, Quality.SDTV, 1, Quality.SDTV, false },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.Bluray720p, false },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.WEBDL720p, false },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 1, Quality.WEBDL720p, false },
            new object[] { Quality.WEBDL1080p, 1, Quality.WEBDL1080p, 1, Quality.WEBDL1080p, false }
        };

        public static object[] IsUpgradeTestCasesLanguages =
        {
            new object[] { Quality.SDTV, 1, Language.English, Quality.SDTV, 2, Language.English, Quality.SDTV, Language.Spanish, true },
            new object[] { Quality.SDTV, 1, Language.English, Quality.SDTV, 1, Language.Spanish, Quality.SDTV, Language.Spanish, true },
            new object[] { Quality.WEBDL720p, 1, Language.French, Quality.WEBDL720p, 2, Language.English, Quality.WEBDL720p, Language.Spanish, true },
            new object[] { Quality.SDTV, 1, Language.English, Quality.SDTV, 1, Language.English, Quality.SDTV, Language.English, false },
            new object[] { Quality.WEBDL720p, 1, Language.English, Quality.HDTV720p, 2, Language.Spanish, Quality.Bluray720p, Language.Spanish, false },
            new object[] { Quality.WEBDL720p, 1, Language.Spanish, Quality.HDTV720p, 2, Language.French, Quality.WEBDL720p, Language.Spanish, false }
        };

        private void GivenAutoDownloadPropers(ProperDownloadTypes type)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadPropersAndRepacks)
                  .Returns(type);
        }

        [Test]
        [TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(current, new Revision(version: currentVersion)),
                       new List<CustomFormat>(),
                       new QualityModel(newQuality, new Revision(version: newVersion)),
                       new List<CustomFormat>())
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_true_if_proper_and_download_propers_is_do_not_download()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotUpgrade);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DVD, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DVD, new Revision(version: 2)),
                       new List<CustomFormat>())
                    .Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_do_not_prefer()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotPrefer);

            var profile = new QualityProfile
                          {
                              Items = Qualities.QualityFixture.GetDefaultQualities(),
                          };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DVD, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DVD, new Revision(version: 2)),
                       new List<CustomFormat>())
                   .Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new QualityProfile
                          {
                              Items = Qualities.QualityFixture.GetDefaultQualities(),
                          };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().BeFalse();
        }
    }
}
