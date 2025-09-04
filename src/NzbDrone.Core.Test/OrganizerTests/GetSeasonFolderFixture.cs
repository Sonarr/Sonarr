using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class GetSeasonFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("Venture Bros.", 1, null, "{Series.Title}.{season:00}", "Venture.Bros.01")]
        [TestCase("Venture Bros.", 1, null, "{Series Title} Season {season:00}", "Venture Bros. Season 01")]
        [TestCase("Series Title?", 1, null, "{Series Title} Season {season:00}", "Series Title! Season 01")]
        [TestCase("Series Title?", 1, null, "{Series Title} Season {season:00} ({Season Year})", "Series Title! Season 01 (Unknown)")]
        [TestCase("Series Title?", 1, 2025, "{Series Title} Season {season:00} ({Season Year})", "Series Title! Season 01 (2025)")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string seriesTitle, int seasonNumber, int? seasonYear, string format, string expected)
        {
            _namingConfig.SeasonFolderFormat = format;

            var series = new Series { Title = seriesTitle };

            Subject.GetSeasonFolder(series, seasonNumber, seasonYear, _namingConfig).Should().Be(expected);
        }
    }
}
