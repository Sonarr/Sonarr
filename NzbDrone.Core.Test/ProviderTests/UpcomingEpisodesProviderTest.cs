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

    public class UpcomingEpisodesProviderTest : CoreTest
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
        public void Get_Yesterday()
        {
            var result = Mocker.Resolve<UpcomingEpisodesProvider>().Yesterday();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.AirDate.Value.Date == DateTime.Today.AddDays(-1).Date);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().Be(series.SeriesId);
        }

        [Test]
        public void Get_Today()
        {
            //Act
            var result = Mocker.Resolve<UpcomingEpisodesProvider>().Today();

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.AirDate.Value.Date == DateTime.Today.Date);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().Be(series.SeriesId);
        }

        [Test]
        public void Get_Tomorrow()
        {
            var result = Mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow();

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.AirDate.Value.Date == DateTime.Today.AddDays(1).Date);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().Be(series.SeriesId);
        }

        [Test]
        public void Get_Week()
        {
            var result = Mocker.Resolve<UpcomingEpisodesProvider>().Week();

            //Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => c.Series != null && c.SeriesId == series.SeriesId);
        }

        [Test]
        public void Get_Yesterday_should_skip_ingored()
        {
            WithIgnoredEpisodes();
            Mocker.Resolve<UpcomingEpisodesProvider>().Yesterday().Should().BeEmpty();
        }

        [Test]
        public void Get_Today_should_skip_ingored()
        {
            WithIgnoredEpisodes();
            Mocker.Resolve<UpcomingEpisodesProvider>().Today().Should().BeEmpty();
        }

        [Test]
        public void Get_Tomorrow_should_skip_ingored()
        {
            WithIgnoredEpisodes();
            Mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow().Should().BeEmpty();
        }

        [Test]
        public void Get_Week_should_skip_ingored()
        {
            WithIgnoredEpisodes();
            Mocker.Resolve<UpcomingEpisodesProvider>().Week().Should().BeEmpty();
        }

        [Test]
        public void Get_Today_should_skip_unmonitored_series()
        {
            WithIgnoredSeries();
            Mocker.Resolve<UpcomingEpisodesProvider>().Today().Should().BeEmpty();
        }

        [Test]
        public void Get_Tomoroww_should_skip_unmonitored_series()
        {
            WithIgnoredSeries();
            Mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow().Should().BeEmpty();
        }

        [Test]
        public void Get_Week_should_skip_unmonitored_series()
        {
            WithIgnoredSeries();
            Mocker.Resolve<UpcomingEpisodesProvider>().Week().Should().BeEmpty();
        }
    }
}
