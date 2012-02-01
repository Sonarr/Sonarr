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

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(historyItem);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().AllItems();

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

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(historyItems);
            db.InsertMany(episodes);
            db.Insert(seriesOne);
            db.Insert(seriesTwo);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().AllItemsWithRelationships();

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

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(historyItem);


            //Act
            db.Fetch<History>().Should().HaveCount(10);
            Mocker.Resolve<HistoryProvider>().Purge();

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

            
            var db = TestDbHelper.GetEmptyDatabase();
            Mocker.SetConstant(db);

            db.InsertMany(historyItem);


            //Act
            db.Fetch<History>().Should().HaveCount(30);
            Mocker.Resolve<HistoryProvider>().Trim();

            //Assert
            var result =  db.Fetch<History>();
            result.Should().HaveCount(10);
            result.Should().OnlyContain(s => s.Date > DateTime.Now.AddDays(-30));
        }


        [Test]
        public void GetBestQualityInHistory_no_result()
        {
            WithStrictMocker();

            Mocker.SetConstant(TestDbHelper.GetEmptyDatabase());

            //Act
            var result = Mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(12);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestQualityInHistory_single_result()
        {
            WithStrictMocker();

            var db = TestDbHelper.GetEmptyDatabase();
            var history = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p).Build();

            db.Insert(history);
            Mocker.SetConstant(db);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(history.EpisodeId);

            //Assert
            result.Should().NotBeNull();
            result.QualityType.Should().Be(QualityTypes.Bluray720p);
        }

        [Test]
        public void add_item()
        {
            
            var db = TestDbHelper.GetEmptyDatabase();

            Mocker.SetConstant(db);

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
            Mocker.Resolve<HistoryProvider>().Add(history);

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

        [Test]
        public void IsBlacklisted_should_return_false_if_nzbTitle_doesnt_exist()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = false)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted("Not a Real NZB Title");

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsBlacklisted_should_return_false_if_nzbTitle_is_not_blacklisted()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = false)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(history.NzbTitle);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsBlacklisted_should_return_true_if_nzbTitle_is_blacklisted()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = true)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(history.NzbTitle);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsBlacklisted_should_return_true_if_nzbTitle_is_blacklisted_ignoring_case()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = true)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(history.NzbTitle.ToLower());

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsBlacklisted_should_return_false_if_newzbinId_doesnt_exist()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = false)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(555);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsBlacklisted_should_return_false_if_newzbinId_is_not_blacklisted()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = false)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(history.NewzbinId);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsBlacklisted_should_return_true_if_newzbinId_is_blacklisted()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = true)
                    .Build();

            Db.Insert(history);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().IsBlacklisted(history.NewzbinId);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsBlacklisted_should_throw_if_newzbinId_is_less_than_1()
        {
            Assert.Throws<ArgumentException>(() =>
                    Mocker.Resolve<HistoryProvider>().IsBlacklisted(0)
                );
        }

        [Test]
        public void SetBlacklist_should_set_to_true_when_true_is_passed_in()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = false)
                    .Build();

            Db.Insert(history);

            //Act
            Mocker.Resolve<HistoryProvider>().SetBlacklist(history.HistoryId, true);

            //Assert
            var result = Db.Single<History>(history.HistoryId);
            result.Blacklisted.Should().BeTrue();
        }

        [Test]
        public void SetBlacklist_should_set_to_false_when_false_is_passed_in()
        {
            WithRealDb();

            var history = Builder<History>.CreateNew()
                    .With(h => h.Blacklisted = true)
                    .Build();

            Db.Insert(history);

            //Act
            Mocker.Resolve<HistoryProvider>().SetBlacklist(history.HistoryId, false);

            //Assert
            var result = Db.Single<History>(history.HistoryId);
            result.Blacklisted.Should().BeFalse();
        }

        [Test]
        public void SetBlacklist_should_throw_if_newzbinId_is_less_than_1()
        {
            Assert.Throws<ArgumentException>(() =>
                    Mocker.Resolve<HistoryProvider>().SetBlacklist(0, true)
                );
        }
    }
}