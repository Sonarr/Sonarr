using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class HistoryProviderTest
    {
        [Test]
        public void AllItems()
        {
            //Setup
            var indexer = new Indexer {Enabled = true, Name = "NzbMatrix", Order = 1, RssUrl = "http://www.nzbmatrix.com"};
            var series = new Series
                             {
                                 SeriesId = 5656,
                                 CleanTitle = "rock",
                                 Monitored = true,
                                 Overview = "Series Overview",
                                 Quality = 1,
                                 Title = "30 Rock",
                                 Path = @"C:\Test\TV\30 Rock"
                             };
            var season = new Season{SeasonId = 4321, SeasonNumber = 1, SeriesId = 5656, Monitored = true};
            var episode = new Episode
                              {
                                  AirDate = DateTime.Today.AddDays(-1),
                                  EpisodeId = 1234,
                                  EpisodeNumber = 5,
                                  Language = "English",
                                  Overview = "This is an Overview",
                                  SeasonNumber = 1,
                                  SeasonId = 4321,
                                  Season = season,
                                  SeriesId = 5656
                              };

            var list = new List<History>();
            list.Add(new History
                         {
                             Id = new int(),
                             Date = DateTime.Now,
                             IsProper = false,
                             Quality = 1,
                             Indexer = indexer,
                             Episode = episode,
                             EpisodeId = 1234
                         });

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.All<History>()).Returns(list.AsQueryable());

            var target = new HistoryProvider(repo.Object);

            //Act
            var result = target.AllItems();

            //Assert
            Assert.AreEqual(result.Count(), 1);
        }

        [Test]
        public void Exists_True()
        {
            //Todo: This test fails... Moq Setup doesn't return the expected value
            //Setup
            var indexer = new Indexer { Enabled = true, Name = "NzbMatrix", Order = 1, RssUrl = "http://www.nzbmatrix.com" };
            var series = new Series
            {
                SeriesId = 5656,
                CleanTitle = "rock",
                Monitored = true,
                Overview = "Series Overview",
                Quality = 1,
                Title = "30 Rock",
                Path = @"C:\Test\TV\30 Rock"
            };
            var season = new Season { SeasonId = 4321, SeasonNumber = 1, SeriesId = 5656, Monitored = true };
            var episode = new Episode
            {
                AirDate = DateTime.Today.AddDays(-1),
                EpisodeId = 1234,
                EpisodeNumber = 5,
                Language = "English",
                Overview = "This is an Overview",
                SeasonNumber = 1,
                SeasonId = 4321,
                Season = season,
                SeriesId = 5656
            };

            var list = new List<History>();
            list.Add(new History
            {
                Id = new int(),
                Date = DateTime.Now,
                IsProper = false,
                Quality = 1,
                Indexer = indexer,
                Episode = episode,
                EpisodeId = 1234
            });

            var proper = false;

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.Exists<History>(h => h.EpisodeId == episode.EpisodeId && h.IsProper == proper)).Returns(true);

            var target = new HistoryProvider(repo.Object);

            //Act
            var result = target.Exists(episode, false);

            //Assert
            Assert.AreEqual(result, true);
        }

        [Test]
        public void Exists_False()
        {
            //Todo: This test fails... Moq Setup doesn't return the expected value

            //Setup
            var indexer = new Indexer { Enabled = true, Name = "NzbMatrix", Order = 1, RssUrl = "http://www.nzbmatrix.com" };
            var series = new Series
            {
                SeriesId = 5656,
                CleanTitle = "rock",
                Monitored = true,
                Overview = "Series Overview",
                Quality = 1,
                Title = "30 Rock",
                Path = @"C:\Test\TV\30 Rock"
            };
            var season = new Season { SeasonId = 4321, SeasonNumber = 1, SeriesId = 5656, Monitored = true };
            var episode = new Episode
            {
                AirDate = DateTime.Today.AddDays(-1),
                EpisodeId = 1234,
                EpisodeNumber = 5,
                Language = "English",
                Overview = "This is an Overview",
                SeasonNumber = 1,
                SeasonId = 4321,
                Season = season,
                SeriesId = 5656
            };

            var list = new List<History>();
            list.Add(new History
            {
                Id = new int(),
                Date = DateTime.Now,
                IsProper = false,
                Quality = 1,
                Indexer = indexer,
                Episode = episode,
                EpisodeId = 1234
            });

            var repo = new Mock<IRepository>();
            repo.Setup(r => r.Exists<History>(h => h.Episode == episode && h.IsProper == list[0].IsProper)).Returns(false);

            var target = new HistoryProvider(repo.Object);

            //Act
            var result = target.Exists(episode, true);

            //Assert
            Assert.AreEqual(result, false);
        }
    }
}
