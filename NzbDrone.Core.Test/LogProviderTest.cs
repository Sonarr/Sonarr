// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using FluentAssertions;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class LogProviderTest : TestBase
    {


        [Test]
        public void write_log()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var db = MockLib.GetEmptyDatabase(true);

            var sonicTarget = new DatabaseTarget(db);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();
            //Act

            Logger.Info(message);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Info.Name, logItem.Level);
            Assert.AreEqual("write_log", logItem.Method);
        }


        [Test]
        public void clearLog()
        {
            //setup
            var db = MockLib.GetEmptyDatabase(true);

            var sonicTarget = new DatabaseTarget(db);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();
            //Act

            for (int i = 0; i < 10; i++)
            {
                Logger.Info("Test");
            }



            //Assert
            var provider = new LogProvider(db);
            provider.GetAllLogs().Should().HaveCount(10);
            provider.DeleteAll();
            provider.GetAllLogs().Should().HaveCount(1);
        }



        [Test]
        public void write_log_exception()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            var db = MockLib.GetEmptyDatabase(true);

            var sonicTarget = new DatabaseTarget(db);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
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

            var db = MockLib.GetEmptyDatabase(true);

            var sonicTarget = new DatabaseTarget(db);

            LogManager.Configuration.AddTarget("DbLogger", sonicTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, sonicTarget));
            LogManager.Configuration.Reload();

            Logger Logger = LogManager.GetCurrentClassLogger();

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(ex.Message, logItem.Message);
            Assert.AreEqual(Logger.Name, logItem.Logger);
            Assert.AreEqual(LogLevel.Error.Name, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.Exception);
            ExceptionVerification.ExcpectedErrors(1);
        }





        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            //setup

            Logger Logger = LogManager.GetCurrentClassLogger();
            var epFile = new EpisodeFile();
            Logger.Trace("File {0} no longer exists on disk. removing from database.", epFile.Path);

            epFile.Path.Should().BeNull();
        }

    }
}