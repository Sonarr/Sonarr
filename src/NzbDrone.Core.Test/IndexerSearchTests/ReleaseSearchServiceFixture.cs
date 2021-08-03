using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class ReleaseSearchServiceFixture : CoreTest<ReleaseSearchService>
    {
        private Mock<IIndexer> _mockIndexer;
        private Series _xemSeries;
        private List<Episode> _xemEpisodes;

        [SetUp]
        public void SetUp()
        {
            _mockIndexer = Mocker.GetMock<IIndexer>();
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition { Id = 1 });
            _mockIndexer.SetupGet(s => s.SupportsSearch).Returns(true);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.AutomaticSearchEnabled(true))
                  .Returns(new List<IIndexer> { _mockIndexer.Object });

            Mocker.GetMock<IMakeDownloadDecision>()
                .Setup(s => s.GetSearchDecision(It.IsAny<List<Parser.Model.ReleaseInfo>>(), It.IsAny<SearchCriteriaBase>()))
                .Returns(new List<DownloadDecision>());

            _xemSeries = Builder<Series>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .Build();

            _xemEpisodes = new List<Episode>();

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetSeries(_xemSeries.Id))
                .Returns(_xemSeries);

            Mocker.GetMock<IEpisodeService>()
                .Setup(v => v.GetEpisodesBySeason(_xemSeries.Id, It.IsAny<int>()))
                .Returns<int, int>((i, j) => _xemEpisodes.Where(d => d.SeasonNumber == j).ToList());

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindByTvdbId(It.IsAny<int>()))
                  .Returns(new List<SceneMapping>());

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneNames(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                  .Returns(new List<string>());
        }

        private void WithEpisode(int seasonNumber, int episodeNumber, int? sceneSeasonNumber, int? sceneEpisodeNumber, string airDate = null)
        {
            var episode = Builder<Episode>.CreateNew()
                .With(v => v.SeriesId == _xemSeries.Id)
                .With(v => v.Series == _xemSeries)
                .With(v => v.SeasonNumber, seasonNumber)
                .With(v => v.EpisodeNumber, episodeNumber)
                .With(v => v.SceneSeasonNumber, sceneSeasonNumber)
                .With(v => v.SceneEpisodeNumber, sceneEpisodeNumber)
                .With(v => v.AirDate = airDate ?? $"{2000 + seasonNumber}-{(episodeNumber % 12) + 1:00}-05")
                .With(v => v.AirDateUtc = DateTime.ParseExact(v.AirDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime())
                .With(v => v.Monitored = true)
                .Build();

            _xemEpisodes.Add(episode);
        }

        private void WithEpisodes()
        {
            // Season 1 maps to Scene Season 2 (one-to-one)
            WithEpisode(1, 12, 2, 3);
            WithEpisode(1, 13, 2, 4);

            // Season 2 maps to Scene Season 3 & 4 (one-to-one)
            WithEpisode(2, 1, 3, 11);
            WithEpisode(2, 2, 3, 12);
            WithEpisode(2, 3, 4, 11);
            WithEpisode(2, 4, 4, 12);

            // Season 3 maps to Scene Season 5 (partial)
            // Season 4 maps to Scene Season 5 & 6 (partial)
            WithEpisode(3, 1, 5, 11);
            WithEpisode(3, 2, 5, 12);
            WithEpisode(4, 1, 5, 13);
            WithEpisode(4, 2, 5, 14);
            WithEpisode(4, 3, 6, 11);
            WithEpisode(5, 1, 6, 12);

            // Season 7+ maps normally, so no mapping specified.
            WithEpisode(7, 1, null, null);
            WithEpisode(7, 2, null, null);
        }

        private List<SearchCriteriaBase> WatchForSearchCriteria()
        {
            var result = new List<SearchCriteriaBase>();

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SingleEpisodeSearchCriteria>()))
                .Callback<SingleEpisodeSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SeasonSearchCriteria>()))
                .Callback<SeasonSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<DailyEpisodeSearchCriteria>()))
                .Callback<DailyEpisodeSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<DailySeasonSearchCriteria>()))
                .Callback<DailySeasonSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<AnimeEpisodeSearchCriteria>()))
                .Callback<AnimeEpisodeSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<SpecialEpisodeSearchCriteria>()))
                .Callback<SpecialEpisodeSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            return result;
        }

        [Test]
        public void Tags_IndexerTags_SeriesNoTags_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 3 }
            });

            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true, false);

            var criteria = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void Tags_IndexerNoTags_SeriesTags_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1
            });

            _xemSeries = Builder<Series>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3 })
                .Build();

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetSeries(_xemSeries.Id))
                .Returns(_xemSeries);

            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true, false);

            var criteria = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public void Tags_IndexerAndSeriesTagsMatch_IndexerIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _xemSeries = Builder<Series>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 3, 4, 5 })
                .Build();

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetSeries(_xemSeries.Id))
                .Returns(_xemSeries);

            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true, false);

            var criteria = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
        }

        [Test]
        public void Tags_IndexerAndSeriesTagsMismatch_IndexerNotIncluded()
        {
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition
            {
                Id = 1,
                Tags = new HashSet<int> { 1, 2, 3 }
            });

            _xemSeries = Builder<Series>.CreateNew()
                .With(v => v.UseSceneNumbering = true)
                .With(v => v.Monitored = true)
                .With(v => v.Tags = new HashSet<int> { 4, 5, 6 })
                .Build();

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetSeries(_xemSeries.Id))
                .Returns(_xemSeries);

            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true, false);

            var criteria = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void scene_episodesearch()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true, false);

            var criteria = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].SeasonNumber.Should().Be(2);
            criteria[0].EpisodeNumber.Should().Be(3);
        }

        [Test]
        public void scene_seasonsearch()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 1, false, false, true, false);

            var criteria = allCriteria.OfType<SeasonSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].SeasonNumber.Should().Be(2);
        }

        [Test]
        public void scene_seasonsearch_should_search_multiple_seasons()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 2, false, false, true, false);

            var criteria = allCriteria.OfType<SeasonSearchCriteria>().ToList();

            criteria.Count.Should().Be(2);
            criteria[0].SeasonNumber.Should().Be(3);
            criteria[1].SeasonNumber.Should().Be(4);
        }

        [Test]
        public void scene_seasonsearch_should_search_single_episode_if_possible()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 4, false, false, true, false);

            var criteria1 = allCriteria.OfType<SeasonSearchCriteria>().ToList();
            var criteria2 = allCriteria.OfType<SingleEpisodeSearchCriteria>().ToList();

            criteria1.Count.Should().Be(1);
            criteria1[0].SeasonNumber.Should().Be(5);

            criteria2.Count.Should().Be(1);
            criteria2[0].SeasonNumber.Should().Be(6);
            criteria2[0].EpisodeNumber.Should().Be(11);
        }

        [Test]
        public void scene_seasonsearch_should_use_seasonnumber_if_no_scene_number_is_available()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 7, false, false, true, false);

            var criteria = allCriteria.OfType<SeasonSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].SeasonNumber.Should().Be(7);
        }

        [Test]
        public void season_search_for_anime_should_search_for_each_monitored_episode()
        {
            WithEpisodes();
            _xemSeries.SeriesType = SeriesTypes.Anime;
            _xemEpisodes.ForEach(e => e.EpisodeFileId = 0);

            var seasonNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, true, false, true, false);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(_xemEpisodes.Count(e => e.SeasonNumber == seasonNumber));
        }

        [Test]
        public void season_search_for_anime_should_not_search_for_unmonitored_episodes()
        {
            WithEpisodes();
            _xemSeries.SeriesType = SeriesTypes.Anime;
            _xemEpisodes.ForEach(e => e.Monitored = false);
            _xemEpisodes.ForEach(e => e.EpisodeFileId = 0);

            var seasonNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, false, true, true, false);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void season_search_for_anime_should_not_search_for_unaired_episodes()
        {
            WithEpisodes();
            _xemSeries.SeriesType = SeriesTypes.Anime;
            _xemEpisodes.ForEach(e => e.AirDateUtc = DateTime.UtcNow.AddDays(5));
            _xemEpisodes.ForEach(e => e.EpisodeFileId = 0);

            var seasonNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, false, false, true, false);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void season_search_for_anime_should_not_search_for_episodes_with_files()
        {
            WithEpisodes();
            _xemSeries.SeriesType = SeriesTypes.Anime;
            _xemEpisodes.ForEach(e => e.EpisodeFileId = 1);

            var seasonNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, true, false, true, false);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void season_search_for_anime_should_set_isSeasonSearch_flag()
        {
            WithEpisodes();
            _xemSeries.SeriesType = SeriesTypes.Anime;
            _xemEpisodes.ForEach(e => e.EpisodeFileId = 0);

            var seasonNumber = 1;
            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, true, false, true, false);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(_xemEpisodes.Count(e => e.SeasonNumber == seasonNumber));
            criteria.ForEach(c => c.IsSeasonSearch.Should().BeTrue());
        }

        [Test]
        public void season_search_for_daily_should_search_multiple_years()
        {
            WithEpisode(1, 1, null, null, "2005-12-30");
            WithEpisode(1, 2, null, null, "2005-12-31");
            WithEpisode(1, 3, null, null, "2006-01-01");
            WithEpisode(1, 4, null, null, "2006-01-02");
            _xemSeries.SeriesType = SeriesTypes.Daily;

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 1, false, false, true, false);

            var criteria = allCriteria.OfType<DailySeasonSearchCriteria>().ToList();

            criteria.Count.Should().Be(2);
            criteria[0].Year.Should().Be(2005);
            criteria[1].Year.Should().Be(2006);
        }

        [Test]
        public void season_search_for_daily_should_search_single_episode_if_possible()
        {
            WithEpisode(1, 1, null, null, "2005-12-30");
            WithEpisode(1, 2, null, null, "2005-12-31");
            WithEpisode(1, 3, null, null, "2006-01-01");
            _xemSeries.SeriesType = SeriesTypes.Daily;

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 1, false, false, true, false);

            var criteria1 = allCriteria.OfType<DailySeasonSearchCriteria>().ToList();
            var criteria2 = allCriteria.OfType<DailyEpisodeSearchCriteria>().ToList();

            criteria1.Count.Should().Be(1);
            criteria1[0].Year.Should().Be(2005);

            criteria2.Count.Should().Be(1);
            criteria2[0].AirDate.Should().Be(new DateTime(2006, 1, 1));
        }

        [Test]
        public void season_search_for_daily_should_not_search_for_unmonitored_episodes()
        {
            WithEpisode(1, 1, null, null, "2005-12-30");
            WithEpisode(1, 2, null, null, "2005-12-31");
            WithEpisode(1, 3, null, null, "2006-01-01");
            _xemSeries.SeriesType = SeriesTypes.Daily;
            _xemEpisodes[0].Monitored = false;

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 1, false, true, true, false);

            var criteria1 = allCriteria.OfType<DailySeasonSearchCriteria>().ToList();
            var criteria2 = allCriteria.OfType<DailyEpisodeSearchCriteria>().ToList();

            criteria1.Should().HaveCount(0);
            criteria2.Should().HaveCount(2);
        }

        [Test]
        public void getscenenames_should_use_seasonnumber_if_no_scene_seasonnumber_is_available()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 7, false, false, true, false);

            Mocker.GetMock<ISceneMappingService>()
                  .Verify(v => v.FindByTvdbId(_xemSeries.Id), Times.Once());

            allCriteria.Should().HaveCount(1);
            allCriteria.First().Should().BeOfType<SeasonSearchCriteria>();
            allCriteria.First().As<SeasonSearchCriteria>().SeasonNumber.Should().Be(7);
        }
    }
}
