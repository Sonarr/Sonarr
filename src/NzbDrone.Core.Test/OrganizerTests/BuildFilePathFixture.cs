using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class BuildFilePathFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season {season:00}", @"C:\Test\30 Rock\Season 01\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season {season}", @"C:\Test\30 Rock\Season 1\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season {season:00}", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season {season}", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "ReallyUglySeasonFolder {season}", @"C:\Test\30 Rock\ReallyUglySeasonFolder 1\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S00E05 - Episode Title", 0, true, "Season {season}", @"C:\Test\30 Rock\MySpecials\30 Rock - S00E05 - Episode Title.mkv")]
        public void CalculateFilePath_SeasonFolder_SingleNumber(string filename, int seasonNumber, bool useSeasonFolder, string seasonFolderFormat, string expectedPath)
        {
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(s => s.Title = "Episode Title")
                .With(s => s.SeasonNumber = seasonNumber)
                .With(s => s.EpisodeNumber = 5)
                .Build().ToList();
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.Path = @"C:\Test\30 Rock".AsOsAgnostic())
                .With(s => s.SeasonFolder = useSeasonFolder)
                .With(s => s.SeriesType = SeriesTypes.Standard)
                .Build();
            var fakeEpisodeFile = Builder<EpisodeFile>.CreateNew()
                .With(s => s.SceneName = filename)
                .Build();

            _namingConfig.SeasonFolderFormat = seasonFolderFormat;
            _namingConfig.SpecialsFolderFormat = "MySpecials";

            Subject.BuildFilePath(fakeEpisodes, fakeSeries, fakeEpisodeFile, ".mkv").Should().Be(expectedPath.AsOsAgnostic());
        }

        [Test]
        public void should_clean_season_folder_when_it_contains_illegal_characters_in_series_title()
        {
            var filename = @"S01E05 - Episode Title";
            var seasonNumber = 1;
            var expectedPath = @"C:\Test\NCIS - Los Angeles\NCIS - Los Angeles Season 1\S01E05 - Episode Title.mkv";

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(s => s.Title = "Episode Title")
                .With(s => s.SeasonNumber = seasonNumber)
                .With(s => s.EpisodeNumber = 5)
                .Build().ToList();
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "NCIS: Los Angeles")
                .With(s => s.Path = @"C:\Test\NCIS - Los Angeles".AsOsAgnostic())
                .With(s => s.SeasonFolder = true)
                .With(s => s.SeriesType = SeriesTypes.Standard)
                .Build();
            var fakeEpisodeFile = Builder<EpisodeFile>.CreateNew()
                .With(s => s.SceneName = filename)
                .Build();

            _namingConfig.SeasonFolderFormat = "{Series Title} Season {season:0}";

            Subject.BuildFilePath(fakeEpisodes, fakeSeries, fakeEpisodeFile, ".mkv").Should().Be(expectedPath.AsOsAgnostic());
        }
    }
}
