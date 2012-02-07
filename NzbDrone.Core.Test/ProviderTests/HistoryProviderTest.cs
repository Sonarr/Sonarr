using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class HistoryProviderTest : CoreTest
    {
        [Test]
        public void AllItems()
        {
            WithRealDb();
            //Setup
            var historyItem = Builder<History>.CreateListOfSize(10).Build();

            Db.InsertMany(historyItem);

            //Act
            var result = Mocker.Resolve<HistoryProvider>().AllItems();

            //Assert
            result.Should().HaveSameCount(historyItem);
        }

        [Test]
        public void AllItemsWithRelationships()
        {
            WithRealDb();
            var seriesOne = Builder<Series>.CreateNew().With(s => s.SeriesId = 12345).Build();
            var seriesTwo = Builder<Series>.CreateNew().With(s => s.SeriesId = 54321).Build();

            var episodes = Builder<Episode>.CreateListOfSize(10).Build();

            var historyItems = Builder<History>.CreateListOfSize(10).TheFirst(5).With(h => h.SeriesId = seriesOne.SeriesId).TheLast(5).With(h => h.SeriesId = seriesTwo.SeriesId).Build();


            Db.InsertMany(historyItems);
            Db.InsertMany(episodes);
            Db.Insert(seriesOne);
            Db.Insert(seriesTwo);

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
            WithRealDb();

            var historyItem = Builder<History>.CreateListOfSize(10).Build();
            Db.InsertMany(historyItem);

            //Act
            Db.Fetch<History>().Should().HaveCount(10);
            Mocker.Resolve<HistoryProvider>().Purge();

            //Assert
            Db.Fetch<History>().Should().HaveCount(0);
        }

        [Test]
        public void Trim_Items()
        {
            WithRealDb();

            var historyItem = Builder<History>.CreateListOfSize(30)
                .TheFirst(10).With(c => c.Date = DateTime.Now)
                .TheNext(20).With(c => c.Date = DateTime.Now.AddDays(-31))
                .Build();

            Db.InsertMany(historyItem);

            //Act
            Db.Fetch<History>().Should().HaveCount(30);
            Mocker.Resolve<HistoryProvider>().Trim();

            //Assert
            var result = Db.Fetch<History>();
            result.Should().HaveCount(10);
            result.Should().OnlyContain(s => s.Date > DateTime.Now.AddDays(-30));
        }


        [Test]
        public void GetBestQualityInHistory_no_result()
        {
            WithRealDb();
            Mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(12, 12, 12).Should().Be(null);
        }

        [Test]
        public void GetBestQualityInHistory_single_result()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(10).Build();
            var historyEpisode = episodes[6];

            var history = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p)
                .With(h => h.IsProper = true)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            Db.Insert(history);
            Db.InsertMany(episodes);

            //Act
            var result = Mocker.Resolve<HistoryProvider>()
                .GetBestQualityInHistory(historyEpisode.SeriesId, historyEpisode.SeasonNumber, historyEpisode.EpisodeNumber);

            //Assert
            result.Should().NotBeNull();
            result.QualityType.Should().Be(QualityTypes.Bluray720p);
            result.Proper.Should().BeTrue();
        }

        [Test]
        public void GetBestQualityInHistory_should_return_highest_result()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(10).Build();
            var historyEpisode = episodes[6];

            var history0 = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.DVD)
                .With(h => h.IsProper = true)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            var history1 = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p)
                .With(h => h.IsProper = false)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            var history2 = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p)
                .With(h => h.IsProper = true)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            var history3 = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.Bluray720p)
                .With(h => h.IsProper = false)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            var history4 = Builder<History>.CreateNew()
                .With(h => h.Quality = QualityTypes.SDTV)
                .With(h => h.IsProper = true)
                .With(h => h.EpisodeId = historyEpisode.EpisodeId)
                .Build();

            Db.Insert(history0);
            Db.Insert(history1);
            Db.Insert(history2);
            Db.Insert(history2);
            Db.Insert(history4);
            Db.InsertMany(episodes);

            //Act
            var result = Mocker.Resolve<HistoryProvider>()
                .GetBestQualityInHistory(historyEpisode.SeriesId, historyEpisode.SeasonNumber, historyEpisode.EpisodeNumber);

            //Assert
            result.Should().NotBeNull();
            result.QualityType.Should().Be(QualityTypes.Bluray720p);
            result.Proper.Should().BeTrue();
        }

        [Test]
        public void add_item()
        {
            WithRealDb();

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
            var storedHistory = Db.Fetch<History>();

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