// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using LogLevel = NLog.LogLevel;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RepoTest : TestBase
    {
        [Test]
        public void to_many__series_to_episode()
        {
            //Arrange
            var fakeSeries = Builder<Series>.CreateNew().With(s => s.SeriesId = 69).Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = 69).Build();

            //Act
            var repo = MockLib.GetEmptyRepository(true);
            repo.Add(fakeSeries);
            repo.Add(fakeEpisode);
            var fetchedSeries = repo.Single<Series>(fakeSeries.SeriesId);

            //Assert
            Assert.AreEqual(fakeSeries.SeriesId, fetchedSeries.SeriesId);
            Assert.AreEqual(fakeSeries.Title, fetchedSeries.Title);


            fetchedSeries.Episodes.Should().HaveCount(1);
            Assert.AreEqual(fetchedSeries.Episodes[0].EpisodeId, fakeEpisode.EpisodeId);
            Assert.AreEqual(fetchedSeries.Episodes[0].SeriesId, fakeEpisode.SeriesId);
            Assert.AreEqual(fetchedSeries.Episodes[0].Title, fakeEpisode.Title);
        }

        [Test]
        public void query_scratch_pad()
        {

            var repo = MockLib.GetEmptyRepository(true);

            repo.All<Episode>().Where(e => !e.Ignored && e.AirDate <= DateTime.Today && e.AirDate.Year > 1900).Select(
                s => s.Title).ToList();
        }


        [Test]
        [Ignore]
        public void episode_proxy_to_string()
        {
            var episode = Builder<Episode>.CreateNew()
                .Build();
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = episode.SeriesId)
                .Build();

            var repo = MockLib.GetEmptyRepository(true);
            repo.Add(episode);
            repo.Add(series);

            //Act

            var result = repo.Single<Episode>(episode.EpisodeId).ToString();

            //Assert
            Console.WriteLine(result);
            result.Should().Contain(series.Title);
            result.Should().Contain(episode.EpisodeNumber.ToString());
            result.Should().Contain(episode.SeasonNumber.ToString());
        }


        [Test]
        [Description(
            "This test confirms that the tvdb id stored in the db is preserved rather than being replaced by an auto incrementing value"
            )]
        public void tvdbid_is_preserved()
        {
            //Arrange
            var sonicRepo = MockLib.GetEmptyRepository(true);
            var series = Builder<Series>.CreateNew().With(c => c.SeriesId = 18).Build();

            //Act
            var addId = sonicRepo.Add(series);

            //Assert
            Assert.AreEqual(18, addId);
            var allSeries = sonicRepo.All<Series>();
            allSeries.Should().HaveCount(1);
            Assert.AreEqual(18, allSeries.First().SeriesId);
        }

        [Test]
        public void enteties_toString()
        {
            Console.WriteLine(new Episode().ToString());
            Console.WriteLine(new Series().ToString());
            Console.WriteLine(new EpisodeFile().ToString());
        }
    }
}