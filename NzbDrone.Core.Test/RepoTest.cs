// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using NLog;
using NLog.Config;
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
            var repo = MockLib.GetEmptyRepository();
            repo.Add(fakeSeries);
            repo.Add(fakeEpisode);
            var fetchedSeries = repo.Single<Series>(fakeSeries.SeriesId);

            //Assert
            Assert.AreEqual(fakeSeries.SeriesId, fetchedSeries.SeriesId);
            Assert.AreEqual(fakeSeries.Title, fetchedSeries.Title);

            Assert.IsNotEmpty(fetchedSeries.Episodes);
            Assert.AreEqual(fetchedSeries.Episodes[0].EpisodeId, fakeEpisode.EpisodeId);
            Assert.AreEqual(fetchedSeries.Episodes[0].SeriesId, fakeEpisode.SeriesId);
            Assert.AreEqual(fetchedSeries.Episodes[0].Title, fakeEpisode.Title);
        }

        [Test]
        public void ToString_test_over_castle_proxy()
        {
            //Arrange
            var fakeSeries = Builder<Series>.CreateNew().With(s => s.SeriesId = 69).Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = 69).Build();

            //Act
            var repo = MockLib.GetEmptyRepository(true);
            repo.Add(fakeSeries);
            repo.Add(fakeEpisode);
            Console.WriteLine("Getting single");
            var fetchedEpisode = repo.Single<Episode>(fakeEpisode.EpisodeId);

            //Assert
            Console.WriteLine("Doing assert");
            Assert.AreEqual(fakeEpisode.EpisodeId, fetchedEpisode.EpisodeId);
            Console.WriteLine("Doing assert");
            Assert.AreEqual(fakeEpisode.Title, fetchedEpisode.Title);

            Console.WriteLine("=======================");
            var ttt= fetchedEpisode.Series;
            Console.WriteLine("=======================");
            var tttd= fetchedEpisode.Series;
            Console.WriteLine("=======================");

            //Assert.Contains(fetchedEpisode.ToString(), fakeSeries.Title);
        }


        [Test]
        [Description(
            "This test confirms that the tvdb id stored in the db is preserved rather than being replaced by an auto incrementing value"
            )]
        public void tvdbid_is_preserved([RandomNumbers(Minimum = 100, Maximum = 999, Count = 1)] int tvdbId)
        {
            //Arrange
            var sonicRepo = MockLib.GetEmptyRepository();
            var series = Builder<Series>.CreateNew().With(c => c.SeriesId = tvdbId).Build();

            //Act
            var addId = sonicRepo.Add(series);

            //Assert
            Assert.AreEqual(tvdbId, addId);
            var allSeries = sonicRepo.All<Series>();
            Assert.IsNotEmpty(allSeries);
            Assert.AreEqual(tvdbId, allSeries.First().SeriesId);
        }

        [Test]
        public void enteties_toString()
        {
            Console.WriteLine(new Episode().ToString());
            Console.WriteLine(new Season().ToString());
            Console.WriteLine(new Series().ToString());
            Console.WriteLine(new EpisodeFile().ToString());
        }

        [Test]
        public void write_log()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();
            //Act

            Logger.Info(message);

            //Assert
            Assert.IsNotEmpty(sonicRepo.All<Log>());
            Assert.Count(1, sonicRepo.All<Log>());

            var logItem = sonicRepo.All<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Info.Name, logItem.Level);
            Assert.AreEqual("write_log", logItem.Method);


        }

        [Test]
        public void write_log_exception()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);
            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            Assert.IsNotEmpty(sonicRepo.All<Log>());
            Assert.Count(1, sonicRepo.All<Log>());

            var logItem = sonicRepo.All<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(message + ": " + ex.Message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Error.Name, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.Exception);
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void write_log_exception_no_message_should_use_exception_message()
        {
            //setup
            var message = String.Empty;

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);
            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            Assert.IsNotEmpty(sonicRepo.All<Log>());
            Assert.Count(1, sonicRepo.All<Log>());

            var logItem = sonicRepo.All<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(ex.Message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Error.Name, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.Exception);
            ExceptionVerification.ExcpectedErrors(1);
        }
    }
}