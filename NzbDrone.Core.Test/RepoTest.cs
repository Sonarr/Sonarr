using System;
using System.Linq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using NLog;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using LogLevel = NzbDrone.Core.Instrumentation.LogLevel;
using NLog.Config;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RepoTest
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
        [Description("This test confirms that the tvdb id stored in the db is preserved rather than being replaced by an auto incrementing value")]
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
        }

        [Test]
        public void write_log()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, sonicTarget));
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
            Assert.AreEqual(LogLevel.Info, logItem.Level);
        }

        [Test]
        public void write_log_exception()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);
            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, sonicTarget));
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
            Assert.AreEqual(message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Error, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.ExceptionString);
            Assert.AreEqual(ex.Message, logItem.ExceptionMessage);
        }

        [Test]
        public void write_log_exception_no_message_should_use_exception_message()
        {
            //setup
            var message = String.Empty;

            var sonicRepo = MockLib.GetEmptyRepository();

            var sonicTarget = new SubsonicTarget(sonicRepo);
            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, sonicTarget));
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
            Assert.AreEqual(LogLevel.Error, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.ExceptionString);
            Assert.AreEqual(ex.Message, logItem.ExceptionMessage);
        }
    }
}
