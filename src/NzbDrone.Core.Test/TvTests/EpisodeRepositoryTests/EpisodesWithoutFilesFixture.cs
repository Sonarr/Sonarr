using System;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesWithoutFilesFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _monitoredSeries;
        private Series _unmonitoredSeries;
        private PagingSpec<Episode> _pagingSpec;

        [SetUp]
        public async Task Setup()
        {
            _monitoredSeries = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.TvRageId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .With(s => s.TitleSlug = "Title3")
                                        .Build();

            _unmonitoredSeries = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.TvdbId = RandomNumber)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = false)
                                        .With(s => s.TitleSlug = "Title2")
                                        .Build();

            var monitoredSeries = await Db.InsertAsync(_monitoredSeries);
            _monitoredSeries.Id = monitoredSeries.Id;
            var unmonitoredSeries = await Db.InsertAsync(_unmonitoredSeries);
            _unmonitoredSeries.Id = unmonitoredSeries.Id;

            _pagingSpec = new PagingSpec<Episode>
            {
                Page = 1,
                PageSize = 10,
                SortKey = "AirDate",
                SortDirection = SortDirection.Ascending
            };

            var monitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unmonitoredSeriesEpisodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _unmonitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(-5))
                                           .With(e => e.Monitored = true)
                                           .TheFirst(1)
                                           .With(e => e.Monitored = false)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            var unairedEpisodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = _monitoredSeries.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .With(e => e.AirDateUtc = DateTime.Now.AddDays(5))
                                           .With(e => e.Monitored = true)
                                           .Build();

            await Db.InsertManyAsync(monitoredSeriesEpisodes);
            await Db.InsertManyAsync(unmonitoredSeriesEpisodes);
            await Db.InsertManyAsync(unairedEpisodes);
        }

        private void GivenMonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == true && e.Series.Monitored == true);
        }

        private void GivenUnmonitoredFilterExpression()
        {
            _pagingSpec.FilterExpressions.Add(e => e.Monitored == false || e.Series.Monitored == false);
        }

        [Test]
        public async Task should_get_monitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, false);

            episodes.Records.Should().HaveCount(1);
        }

        [Test]
        [Ignore("Specials not implemented")]
        public async Task should_get_episode_including_specials()
        {
            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, true);

            episodes.Records.Should().HaveCount(2);
        }

        [Test]
        public async Task should_not_include_unmonitored_episodes()
        {
            GivenMonitoredFilterExpression();

            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, false);

            episodes.Records.Should().NotContain(e => e.Monitored == false);
        }

        [Test]
        public async Task should_not_contain_unmonitored_series()
        {
            GivenMonitoredFilterExpression();

            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, false);

            episodes.Records.Should().NotContain(e => e.SeriesId == _unmonitoredSeries.Id);
        }

        [Test]
        public async Task should_not_return_unaired()
        {
            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, false);

            episodes.TotalRecords.Should().Be(4);
        }

        [Test]
        public async Task should_not_return_episodes_on_air()
        {
            var onAirEpisode = Builder<Episode>.CreateNew()
                                               .With(e => e.Id = 0)
                                               .With(e => e.SeriesId = _monitoredSeries.Id)
                                               .With(e => e.EpisodeFileId = 0)
                                               .With(e => e.AirDateUtc = DateTime.Now.AddMinutes(-15))
                                               .With(e => e.Monitored = true)
                                               .Build();

            await Db.InsertAsync(onAirEpisode);

            var episodes = await Subject.EpisodesWithoutFilesAsync(_pagingSpec, false);

            episodes.TotalRecords.Should().Be(4);
            episodes.Records.Where(e => e.Id == onAirEpisode.Id).Should().BeEmpty();
        }
    }
}
