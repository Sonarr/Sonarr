using System.Collections.Generic;
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

        [TestCase("Venture Bros.", 1, "{Series.Title}.{season:00}", "Venture.Bros.01")]
        [TestCase("Venture Bros.", 1, "{Series Title} Season {season:00}", "Venture Bros. Season 01")]
        [TestCase("Series Title?", 1, "{Series Title} Season {season:00}", "Series Title! Season 01")]
        [TestCase("Series Title?", 1, "Season {season:00} - {Season Title}", "Season 01 - Part 1!")]
        [TestCase("Series Title?", 1, "Season {season:00} - {Season CleanTitle}", "Season 01 - Part 1")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string seriesTitle, int seasonNumber, string format, string expected)
        {
            _namingConfig.SeasonFolderFormat = format;

            var series = new Series
            {
                Title = seriesTitle, Seasons = new List<Season>
                {
                    new Season
                    {
                        SeasonNumber = seasonNumber,
                        Title = $"Part {seasonNumber}?"
                    }
                }
            };

            Subject.GetSeasonFolder(series, seasonNumber, _namingConfig).Should().Be(expected);
        }

        [Test]
        public void should_conditionally_add_title()
        {
            _namingConfig.SeasonFolderFormat = "Season {season:00}{ - SeasonTitle}";

            var seasonNumber = 1;

            var series = new Series
            {
                Title = "Series Title",
                Seasons = new List<Season>
                {
                    new Season
                    {
                        SeasonNumber = seasonNumber,
                        Title = null
                    }
                }
            };

            Subject.GetSeasonFolder(series, seasonNumber, _namingConfig).Should().Be("Season 01");
        }

        [TestCase("Season {season:00} - {Season Title}")]
        [TestCase("Season {season:00} - {Season CleanTitle}")]
        public void should_not_throw_when_season_has_no_title(string format)
        {
            _namingConfig.SeasonFolderFormat = format;

            var seasonNumber = 1;

            var series = new Series
            {
                Title = "Series Title",
                Seasons = new List<Season>
                {
                    new Season
                    {
                        SeasonNumber = seasonNumber,
                        Title = null
                    }
                }
            };

            Subject.GetSeasonFolder(series, seasonNumber, _namingConfig).Should().Be("Season 01");
        }

        [Test]
        public void should_truncate_from_beginning()
        {
            _namingConfig.SeasonFolderFormat = "Season {season:00}{ - SeasonTitle:10}";

            var seasonNumber = 1;

            var series = new Series
            {
                Title = "Series Title",
                Seasons = new List<Season>
                {
                    new Season
                    {
                        SeasonNumber = seasonNumber,
                        Title = "Partial Title"
                    }
                }
            };

            Subject.GetSeasonFolder(series, seasonNumber, _namingConfig).Should().Be("Season 01 - Partial...");
        }

        [Test]
        public void should_truncate_from_from_end()
        {
            _namingConfig.SeasonFolderFormat = "Season {season:00}{ - SeasonTitle:-8}";

            var seasonNumber = 1;

            var series = new Series
            {
                Title = "Series Title",
                Seasons = new List<Season>
                {
                    new Season
                    {
                        SeasonNumber = seasonNumber,
                        Title = "Partial Title"
                    }
                }
            };

            Subject.GetSeasonFolder(series, seasonNumber, _namingConfig).Should().Be("Season 01 - ...Title");
        }
    }
}
