// ReSharper disable RedundantUsingDirective

using System;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class LogProviderTest : TestBase
    {

        private const string loggerName = "Core.Test.ProviderTests.LogProviderTest";

        private static IDatabase db;
        Logger Logger;

        [SetUp]
        public void Setup()
        {
            db = MockLib.GetEmptyDatabase(true);
            LogConfiguration.RegisterDatabaseLogger(new DatabaseTarget(db));
            Logger = LogManager.GetCurrentClassLogger();
        }

        [Test]
        public void write_log()
        {
            //setup
            var message = Guid.NewGuid().ToString();

            Logger.Info(message);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(message, logItem.Message);
            Assert.AreEqual(loggerName, logItem.Logger);

            Logger.Name.Should().EndWith(logItem.Logger);

            Assert.AreEqual(LogLevel.Info.Name, logItem.Level);
            Assert.AreEqual("write_log", logItem.Method);
        }

        [Test]
        public void write_long_log()
        {
            //setup
            var message = String.Empty;
            for (int i = 0; i < 100; i++)
            {
                message += Guid.NewGuid();
            }


            //Act
            Logger.Info(message);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();

            logItem.Message.Should().HaveLength(message.Length);
            Assert.AreEqual(message, logItem.Message);
        }


        [Test]
        public void clearLog()
        {
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

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(message + ": " + ex.Message, logItem.Message);
            Assert.AreEqual(loggerName, logItem.Logger);
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

            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(message, ex);

            //Assert
            db.Fetch<Log>().Should().HaveCount(1);

            var logItem = db.Fetch<Log>().First();
            Assert.AreNotEqual(new DateTime(), logItem.Time);
            Assert.AreEqual(ex.Message, logItem.Message);
            Assert.AreEqual(loggerName, logItem.Logger);
            Assert.AreEqual(LogLevel.Error.Name, logItem.Level);
            Assert.AreEqual(ex.GetType().ToString(), logItem.ExceptionType);
            Assert.AreEqual(ex.ToString(), logItem.Exception);
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            var epFile = new EpisodeFile();
            Logger.Trace("File {0} no longer exists on disk. removing from database.", epFile.Path);

            epFile.Path.Should().BeNull();
        }

        [Test]
        public void top_logs()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeLogs = Builder<Log>.CreateListOfSize(510)

                .Build();

            db.InsertMany(fakeLogs);

            //Act
            var logs = mocker.Resolve<LogProvider>().TopLogs(500);

            //Assert
            logs.Should().HaveCount(501);
            logs.Last().Message.Should().Be(
                "Number of logs currently shown: 500. More may exist, check 'All' to see everything");
        }

        [Test]
        public void top_logs_less_than_number_wanted()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeLogs = Builder<Log>.CreateListOfSize(100)

                .Build();

            db.InsertMany(fakeLogs);

            //Act
            var logs = mocker.Resolve<LogProvider>().TopLogs(500);

            //Assert
            logs.Should().HaveCount(101);
            logs.Last().Message.Should().Be(
                "Number of logs currently shown: 100. More may exist, check 'All' to see everything");
        }

        [Test]
        public void pagedLogs()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeLogs = Builder<Log>.CreateListOfSize(100)

                .Build();

            db.InsertMany(fakeLogs);

            //Act
            var logs = mocker.Resolve<LogProvider>().GetPagedLogs(1, 50);

            //Assert
            logs.Items.Should().HaveCount(50);
            logs.TotalItems.Should().Be(100);
        }
    }
}
