// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpcomingEpisodesProviderTest : TestBase
    {
        private IList<Episode> episodes;
        private Series series;

        [SetUp]
        public new void Setup()
        {
            episodes = Builder<Episode>.CreateListOfSize(6)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.Ignored = false)
                .TheFirst(1)
                .Has(e => e.AirDate = DateTime.Today.AddDays(-1))
                .AndTheNext(1)
                .Has(e => e.AirDate = DateTime.Today)
                .AndTheNext(1)
                .Has(e => e.AirDate = DateTime.Today.AddDays(1))
                .AndTheNext(1)
                .Has(e => e.AirDate = DateTime.Today.AddDays(2))
                .AndTheNext(1)
                .Has(e => e.AirDate = DateTime.Today.AddDays(7))
                .AndTheNext(1)
                .Has(e => e.AirDate = DateTime.Today.AddDays(9))
                .Build();

            series = Builder<Series>.CreateNew().With(s => s.SeriesId = 1).Build();

            base.Setup();
        }

        [Test]
        public void Get_Yesterday()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Yesterday();

            //Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be(episodes.Where(e => e.AirDate == DateTime.Today.AddDays(-1)).First().Title);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Today()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Today();

            //Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be(episodes.Where(e => e.AirDate == DateTime.Today).First().Title);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Tomorrow()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow();

            //Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be(episodes.Where(e => e.AirDate == DateTime.Today.AddDays(1)).First().Title);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Week()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Week();

            //Assert
            result.Should().HaveCount(2);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().NotBe(0);
            result.Last().Series.Should().NotBeNull();
            result.Last().Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Yesterday_skip_ingored()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            episodes.Where(e => e.AirDate == DateTime.Today.AddDays(-1)).Single().Ignored = true;

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Yesterday();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Get_Today_skip_ingored()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            episodes.Where(e => e.AirDate == DateTime.Today).Single().Ignored = true;

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Today();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Get_Tomorrow_skip_ingored()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            episodes.Where(e => e.AirDate == DateTime.Today.AddDays(1)).Single().Ignored = true;

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Get_Week_skip_ingored()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            episodes.Where(e => e.AirDate == DateTime.Today.AddDays(2)).Single().Ignored = true;

            database.InsertMany(episodes);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Week();

            //Assert
            result.Should().HaveCount(1);
            result.First().Series.Should().NotBeNull();
            result.First().Series.SeriesId.Should().NotBe(0);
        }
    }
}
