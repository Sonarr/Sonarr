using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesWhereCutoffUnmetFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _monitoredSeries;
        private Series _unmonitoredSeries;
        private PagingSpec<Episode> _pagingSpec;

        [SetUp]
        public void Setup()
        {
            var qualityProfile = new QualityProfile 
            {  
                Cutoff = Quality.WEBDL720p,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            _monitoredSeries = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.TvRageId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .With(s => s.TitleSlug = "Title3")
                                        .With(s => s.QualityProfile = qualityProfile)
                                        .Build();

            _unmonitoredSeries = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.TvdbId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = false)
                                        .With(s => s.TitleSlug = "Title2")
                                        .With(s => s.QualityProfile = qualityProfile)
                                        .Build();

            _monitoredSeries.Id = Db.Insert(_monitoredSeries).Id;
            _unmonitoredSeries.Id = Db.Insert(_unmonitoredSeries).Id;

            _pagingSpec = new PagingSpec<Episode>
                              {
                                  Page = 1,
                                  PageSize = 10,
                                  SortKey = "AirDate",
                                  SortDirection = SortDirection.Ascending
                              };
            
            var qualityMet = new EpisodeFile { Path = "a", Quality = new QualityModel { Quality = Quality.WEBDL720p } };
            var qualityUnmet = new EpisodeFile { Path = "b", Quality = new QualityModel { Quality = Quality.WEBDL480p } };

            MediaFileRepository fileRepository = Mocker.Resolve<MediaFileRepository>();

            qualityMet = fileRepository.Insert(qualityMet);
            qualityUnmet = fileRepository.Insert(qualityUnmet);

            var monitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMet.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _unmonitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .With(e => e.EpisodeFileId = qualityMet.Id)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();


            var unairedEpisodes           = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .With(e => e.EpisodeFileId = qualityUnmet.Id)
                                           .Build();
            
            Db.InsertMany(monitoredSeriesEpisodes);
            Db.InsertMany(unmonitoredSeriesEpisodes);
            Db.InsertMany(unairedEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpression = e => e.Monitored == true && e.Series.Monitored == true;
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpression = e => e.Monitored == false || e.Series.Monitored == false;
        }

        [Test]
        public void should_get_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var episodes = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, false);

            episodes.Should().HaveCount(1);
        }

        [Test]
        [Ignore("Specials not implemented")]
        public void should_get_episode_including_specials()
        {
            var episodes = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, true);

            episodes.Should().HaveCount(2);
        }

        [Test]
        public void should_not_include_unmonitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var episodes = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, false);

            episodes.Should().NotContain(e => e.Monitored == false);
        }

        [Test]
        public void should_not_contain_unmonitored_series()
        {
            GivenMonitoredFilterExpression();

            var episodes = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, false);

            episodes.Should().NotContain(e => e.SeriesId == _unmonitoredSeries.Id);
        }

        [Test]
        public void should_not_include_cutoff_met_episodes()
        {
            GivenMonitoredFilterExpression();

            var episodes = Subject.EpisodesWhereCutoffUnmet(_pagingSpec, false);

            episodes.Should().NotContain(e => e.EpisodeFile.Value.Quality.Quality == Quality.WEBDL720p);
        }
    }
}
