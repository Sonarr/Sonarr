using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Test.Framework;
using FizzWare.NBuilder;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class NzbSearchServiceFixture : CoreTest<NzbSearchService>
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
                  .Setup(s => s.SearchEnabled())
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
                  .Setup(s => s.GetSceneNames(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                  .Returns(new List<string>());
        }

        private void WithEpisode(int seasonNumber, int episodeNumber, int? sceneSeasonNumber, int? sceneEpisodeNumber)
        {
            var episode = Builder<Episode>.CreateNew()
                .With(v => v.SeriesId == _xemSeries.Id)
                .With(v => v.Series == _xemSeries)
                .With(v => v.SeasonNumber, seasonNumber)
                .With(v => v.EpisodeNumber, episodeNumber)
                .With(v => v.SceneSeasonNumber, sceneSeasonNumber)
                .With(v => v.SceneEpisodeNumber, sceneEpisodeNumber)
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

            _mockIndexer.Setup(v => v.Fetch(It.IsAny<AnimeEpisodeSearchCriteria>()))
                .Callback<AnimeEpisodeSearchCriteria>(s => result.Add(s))
                .Returns(new List<Parser.Model.ReleaseInfo>());

            return result;
        }

        [Test]
        public void scene_episodesearch()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.EpisodeSearch(_xemEpisodes.First(), true);

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

            Subject.SeasonSearch(_xemSeries.Id, 1, false, true);

            var criteria = allCriteria.OfType<SeasonSearchCriteria>().ToList();

            criteria.Count.Should().Be(1);
            criteria[0].SeasonNumber.Should().Be(2);
        }

        [Test]
        public void scene_seasonsearch_should_search_multiple_seasons()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 2, false, true);

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

            Subject.SeasonSearch(_xemSeries.Id, 4, false, true);

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

            Subject.SeasonSearch(_xemSeries.Id, 7, false, true);

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

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, true, true);

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

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, false, true);

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

            Subject.SeasonSearch(_xemSeries.Id, seasonNumber, true, true);

            var criteria = allCriteria.OfType<AnimeEpisodeSearchCriteria>().ToList();

            criteria.Count.Should().Be(0);
        }

        [Test]
        public void getscenenames_should_use_seasonnumber_if_no_scene_seasonnumber_is_available()
        {
            WithEpisodes();

            var allCriteria = WatchForSearchCriteria();

            Subject.SeasonSearch(_xemSeries.Id, 7, false, true);

            Mocker.GetMock<ISceneMappingService>()
                  .Verify(v => v.GetSceneNames(_xemSeries.Id, It.Is<List<int>>(l => l.Contains(7)), It.Is<List<int>>(l => l.Contains(7))), Times.Once());
        }
    }
}
