using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class QualityUpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
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

        [Test, TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(true);


            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            var langProfile = new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.English
            };

            Subject.IsUpgradable(profile, langProfile, new QualityModel(current, new Revision(version: currentVersion)), Language.English, new QualityModel(newQuality, new Revision(version: newVersion)), Language.English)
                    .Should().Be(expected);
        }

        [Test, TestCaseSource("IsUpgradeTestCasesLanguages")]
        public void IsUpgradeTestLanguage(Quality current, int currentVersion, Language currentLanguage, Quality newQuality, int newVersion, Language newLanguage, Quality cutoff, Language languageCutoff, bool expected)
        {
            GivenAutoDownloadPropers(true);

            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                Cutoff = cutoff.Id,
            };

            var langProfile = new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = languageCutoff
            };

            Subject.IsUpgradable(profile, langProfile, new QualityModel(current, new Revision(version: currentVersion)), currentLanguage, new QualityModel(newQuality, new Revision(version: newVersion)), newLanguage)
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_false()
        {
            GivenAutoDownloadPropers(false);

            var profile = new Profile 
            { 
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            var langProfile = new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.English
            };


            Subject.IsUpgradable(profile, langProfile, new QualityModel(Quality.DVD, new Revision(version: 2)), Language.English, new QualityModel(Quality.DVD, new Revision(version: 1)), Language.English)
                    .Should().BeFalse();
        }
    }
}