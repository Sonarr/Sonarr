// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;

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
            Season season = new Season { SeasonId = 4321, SeasonNumber = 1, SeriesId = 5656, Monitored = true };
            var episode = new Episode
                              {
                                  AirDate = DateTime.Today.AddDays(-1),
                                  EpisodeId = 1234,
                                  EpisodeNumber = 5,
                                  Overview = "This is an Overview",
                                  SeasonNumber = 1,
                                  SeasonId = 4321,
                                  Season = season,
                                  SeriesId = 5656
                              };

            var list = new List<History>
                           {
                               new History
                                   {
                                       HistoryId = new int(),
                                       Date = DateTime.Now,
                                       IsProper = false,
                                       Quality = QualityTypes.SDTV,
                                       EpisodeId = episode.EpisodeId
                                   }
                           };

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.All<History>()).Returns(list.AsQueryable());

            var target = new HistoryProvider(repo.Object);

            //Act
            var result = target.AllItems();

            //Assert
            Assert.AreEqual(result.Count(), 1);
        }


        [Test]
        public void GetBestQualityInHistory_no_result()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var result = mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(12);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestQualityInHistory_single_result()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);


            var repo = MockLib.GetEmptyRepository();
            var history = Builder<History>.CreateNew().Build();
            repo.Add(history);
            mocker.SetConstant(repo);

            //Act
            var result = mocker.Resolve<HistoryProvider>().GetBestQualityInHistory(history.EpisodeId);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void add_item()
        {
            //Arange
            var mocker = new AutoMoqer();
            var repo = MockLib.GetEmptyRepository();

            mocker.SetConstant(repo);

            var episodes = MockLib.GetFakeEpisodes(1);
            repo.AddMany(episodes);

            var episode = episodes[5];

            var history = new History
                              {
                                  Date = DateTime.Now,
                                  EpisodeId = episode.EpisodeId,
                                  NzbTitle = "my title",
                                  Indexer = "Fake Indexer"
                              };

            //Act
            mocker.Resolve<HistoryProvider>().Add(history);

            //Assert
            var storedHistory = repo.All<History>();
            var newHistiory = repo.All<History>().First();

            Assert.Count(1, storedHistory);
            Assert.AreEqual(history.Date, newHistiory.Date);
            Assert.AreEqual(history.EpisodeId, newHistiory.EpisodeId);
            Assert.AreEqual(history.NzbTitle, newHistiory.NzbTitle);
            Assert.AreEqual(history.Indexer, newHistiory.Indexer);
        }


    }
}