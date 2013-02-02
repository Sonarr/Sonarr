// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]

    public class UpcomingEpisodesProviderTest : SqlCeTest
    {
        private IList<Episode> episodes;
        private Series series;

        [SetUp]
        public void Setup()
        {
            WithRealDb();

            episodes = Builder<Episode>.CreateListOfSize(6)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.Ignored = false)
                .TheFirst(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .TheNext(1)
                .With(e => e.AirDate = DateTime.Today)
                .TheNext(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(1))
                .TheNext(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(2))
                .TheNext(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(7))
                .TheNext(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(9))
                .Build();

            series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .And(c => c.Monitored = true)
                .Build();


            Db.InsertMany(episodes);
            Db.Insert(series);
        }

        private void WithIgnoredEpisodes()
        {
            episodes.ToList().ForEach(c => c.Ignored = true);
            Db.UpdateMany(episodes);
        }

        private void WithIgnoredSeries()
        {
            series.Monitored = false;
            Db.Update(series);
        }

        [Test]
        public void Get_UpcomingEpisodes()
        {
            var result = Mocker.Resolve<UpcomingEpisodesProvider>().UpcomingEpisodes();

            //Assert
            result.Should().HaveCount(5);
            result.Should().OnlyContain(c => c.Series != null && c.SeriesId == series.SeriesId);
        }

        [Test]
        public void Get_UpcomingEpisodes_should_skip_ingored()
        {
            WithIgnoredEpisodes();
            Mocker.Resolve<UpcomingEpisodesProvider>().UpcomingEpisodes().Should().BeEmpty();
        }

        [Test]
        public void Get_UpcomingEpisodes_should_skip_unmonitored_series()
        {
            WithIgnoredSeries();
            Mocker.Resolve<UpcomingEpisodesProvider>().UpcomingEpisodes().Should().BeEmpty();
        }
    }
}
