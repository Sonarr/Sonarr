// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpcomingEpisodesProviderTest : TestBase
    {
        private Episode yesterday;
        private Episode today;
        private Episode tomorrow;
        private Episode twoDays;
        private Episode sevenDays;
        private Series series;

        [SetUp]
        public new void Setup()
        {
            yesterday = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(-1))
                .With(c => c.Title = "Yesterday")
                .With(c => c.SeriesId = 1)
                .Build();

            today = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today)
                .With(c => c.Title = "Today")
                .With(c => c.SeriesId = 1)
                .Build();

            tomorrow = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(1))
                .With(c => c.Title = "Tomorrow")
                .With(c => c.SeriesId = 1)
                .Build();

            twoDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(2))
                .With(c => c.Title = "Two Days")
                .With(c => c.SeriesId = 1)
                .Build();

            sevenDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(7))
                .With(c => c.Title = "Seven Days")
                .With(c => c.SeriesId = 1)
                .Build();

            sevenDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(8))
                .With(c => c.Title = "Eight Days")
                .With(c => c.SeriesId = 1)
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

            database.Insert(yesterday);
            database.Insert(today);
            database.Insert(tomorrow);
            database.Insert(twoDays);
            database.Insert(sevenDays);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Yesterday();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(yesterday.Title, result[0].Title);
            result[0].Series.Should().NotBeNull();
            result[0].Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Today()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.Insert(yesterday);
            database.Insert(today);
            database.Insert(tomorrow);
            database.Insert(twoDays);
            database.Insert(sevenDays);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Today();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(today.Title, result[0].Title);
            result[0].Series.Should().NotBeNull();
            result[0].Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Tomorrow()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.Insert(yesterday);
            database.Insert(today);
            database.Insert(tomorrow);
            database.Insert(twoDays);
            database.Insert(sevenDays);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(tomorrow.Title, result[0].Title);
            result[0].Series.Should().NotBeNull();
            result[0].Series.SeriesId.Should().NotBe(0);
        }

        [Test]
        public void Get_Week()
        {
            //Setup
            var database = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(database);

            database.Insert(yesterday);
            database.Insert(today);
            database.Insert(tomorrow);
            database.Insert(twoDays);
            database.Insert(sevenDays);
            database.Insert(series);

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Week();

            //Assert
            Assert.AreEqual(2, result.Count);
            result[0].Series.Should().NotBeNull();
            result[0].Series.SeriesId.Should().NotBe(0);
            result[1].Series.Should().NotBeNull();
            result[1].Series.SeriesId.Should().NotBe(0);
        }
    }
}
