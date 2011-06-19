// ReSharper disable RedundantUsingDirective
using System;
using AutoMoq;
using FizzWare.NBuilder;
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

        [SetUp]
        public new void Setup()
        {
            yesterday = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(-1))
                .With(c => c.Title = "Yesterday")
                .Build();

            today = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today)
                .With(c => c.Title = "Today")
                .Build();

            tomorrow = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(1))
                .With(c => c.Title = "Tomorrow")
                .Build();

            twoDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(2))
                .With(c => c.Title = "Two Days")
                .Build();

            sevenDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(7))
                .With(c => c.Title = "Seven Days")
                .Build();

            sevenDays = Builder<Episode>.CreateNew()
                .With(c => c.AirDate = DateTime.Today.AddDays(8))
                .With(c => c.Title = "Eight Days")
                .Build();

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

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Yesterday();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(yesterday.Title, result[0].Title);
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

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Today();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(today.Title, result[0].Title);
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

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Tomorrow();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(tomorrow.Title, result[0].Title);
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

            //Act
            var result = mocker.Resolve<UpcomingEpisodesProvider>().Week();

            //Assert
            Assert.AreEqual(2, result.Count);
        }
    }
}
