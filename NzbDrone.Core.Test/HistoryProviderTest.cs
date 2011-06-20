// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class HistoryProviderTest : TestBase
    {
        [Test]
        public void AllItems()
        {
            //Setup
            var historyItem = Builder<History>.CreateListOfSize(10).Build();

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.InsertMany(historyItem);

            //Act
            var result = mocker.Resolve<HistoryProvider>().AllItems();

            //Assert
            result.Should().HaveSameCount(historyItem);
        }

        [Test]
        public void AllItemsWithRelationships()
        {
            //Setup
            var seriesOne = Builder<Series>.CreateNew().With(s => s.SeriesId = 12345).Build();
            var seriesTwo = Builder<Series>.CreateNew().With(s => s.SeriesId = 54321).Build();

            var episodes = Builder<Episode>.CreateListOfSize(10).Build();

            var historyItems = Builder<History>.CreateListOfSize(10).WhereTheFirst(5).Have(h => h.SeriesId = seriesOne.SeriesId).WhereTheLast(5).Have(h => h.SeriesId = seriesTwo.SeriesId).Build();

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.InsertMany(historyItems);
            db.InsertMany(episodes);
            db.Insert(seriesOne);
            db.Insert(seriesTwo);

            //Act
            var result = mocker.Resolve<HistoryProvider>().AllItemsWithRelationships();

            //Assert
            result.Should().HaveSameCount(historyItems);

            foreach (var history in result)
            {
                Assert.NotNull(history.Episode);
                Assert.That(!String.IsNullOrEmpty(history.SeriesTitle));
            }
        }

        [Test]
        public void PurgeItem()
        {
            //Setup
            var historyItem = Builder<History>.CreateListOfSize(10).Build();

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.InsertMany(historyItem);


            //Act
            db.Fetch<History>().Should().HaveCount(10);
            mocker.Resolve<HistoryProvider>().Purge();

            //Assert
            db.Fetch<History>().Should().HaveCount(0);
        }

        [Test]
        public void Trim_Items()
        {
            //Setup
            var historyItem = Builder<History>.CreateListOfSize(20)
                .WhereTheFirst(10).Have(c => c.Date = DateTime.Now)
                .AndTheNext(10).Have(c => c.Date = DateTime.Now.AddDays(-31))
                .Build();

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.InsertMany(historyItem);


            //Act
            db.Fetch<History>().Should().HaveCount(20);
            mocker.Resolve<HistoryProvider>().Trim();

            //Assert
            db.Fetch<History>().Should().HaveCount(10);
        }


        [Test]
        public void GetBestQualityInHistory_no_result()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.SetConstant(MockLib.GetEmptyDatabase());

            //Act
            var result = mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(12);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestQualityInHistory_single_result()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var db = MockLib.GetEmptyDatabase();
            var history = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p).Build();

            db.Insert(history);
            mocker.SetConstant(db);

            //Act
            var result = mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(history.EpisodeId);

            //Assert
            Assert.IsNotNull(result);
            result.QualityType.Should().Be(QualityTypes.Bluray720p);
        }

        [Test]
        public void add_item()
        {
            //Arange
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();

            mocker.SetConstant(db);

            var episode = Builder<Episode>.CreateNew().Build();

            const QualityTypes quality = QualityTypes.HDTV;
            const bool proper = true;

            var history = new History
                              {
                                  Date = DateTime.Now,
                                  EpisodeId = episode.EpisodeId,
                                  SeriesId = episode.SeriesId,
                                  NzbTitle = "my title",
                                  Indexer = "Fake Indexer",
                                  Quality = quality,
                                  IsProper = proper
                              };

            //Act
            mocker.Resolve<HistoryProvider>().Add(history);

            //Assert
            var storedHistory = db.Fetch<History>();

            storedHistory.Should().HaveCount(1);
            Assert.AreEqual(history.Date, storedHistory.First().Date);
            Assert.AreEqual(history.EpisodeId, storedHistory.First().EpisodeId);
            Assert.AreEqual(history.SeriesId, storedHistory.First().SeriesId);
            Assert.AreEqual(history.NzbTitle, storedHistory.First().NzbTitle);
            Assert.AreEqual(history.Indexer, storedHistory.First().Indexer);
            Assert.AreEqual(history.Quality, storedHistory.First().Quality);
            Assert.AreEqual(history.IsProper, storedHistory.First().IsProper);
        }
    }
}