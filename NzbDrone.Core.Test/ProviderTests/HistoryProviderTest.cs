using System;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class HistoryProviderTest : CoreTest
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

            var historyItems = Builder<History>.CreateListOfSize(10).TheFirst(5).With(h => h.SeriesId = seriesOne.SeriesId).TheLast(5).With(h => h.SeriesId = seriesTwo.SeriesId).Build();

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
            var historyItem = Builder<History>.CreateListOfSize(30)
                .TheFirst(10).With(c => c.Date = DateTime.Now)
                .TheNext(20).With(c => c.Date = DateTime.Now.AddDays(-31))
                .Build();

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.InsertMany(historyItem);


            //Act
            db.Fetch<History>().Should().HaveCount(30);
            mocker.Resolve<HistoryProvider>().Trim();

            //Assert
            var result =  db.Fetch<History>();
            result.Should().HaveCount(10);
            result.Should().OnlyContain(s => s.Date > DateTime.Now.AddDays(-30));
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
            result.Should().NotBeNull();
            result.QualityType.Should().Be(QualityTypes.Bluray720p);
        }

        [Test]
        public void add_item()
        {
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
            history.Date.Should().BeWithin(TimeSpan.FromMinutes(1)).Before(storedHistory.First().Date);
           
            history.EpisodeId.Should().Be(storedHistory.First().EpisodeId);
            history.SeriesId.Should().Be(storedHistory.First().SeriesId);
            history.NzbTitle.Should().Be(storedHistory.First().NzbTitle);
            history.Indexer.Should().Be(storedHistory.First().Indexer);
            history.Quality.Should().Be(storedHistory.First().Quality);
            history.IsProper.Should().Be(storedHistory.First().IsProper);
        }
    }
}