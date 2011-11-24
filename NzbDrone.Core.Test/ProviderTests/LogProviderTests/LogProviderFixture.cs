// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.LogProviderTests
{
    [TestFixture]
    public class LogProviderFixture : CoreTest
    {

        private const string LOGGER_NAME = "Core.Test.ProviderTests.LogProviderTests.LogProviderFixture";

        private static LogDbContext dbContext;
        private static string UniqueMessage;

        Logger Logger;

        [SetUp]
        public void Setup()
        {
            WithRealDb();

            dbContext = Connection.GetLogDbContext(TestDbHelper.ConnectionString);

            new DatabaseTarget(Db).Register();
            Logger = LogManager.GetCurrentClassLogger();

            UniqueMessage = "Unique message: " + Guid.NewGuid().ToString();
        }

        [Test]
        public void write_log()
        {
            Logger.Info(UniqueMessage);

            //Assert
            var logItem = Db.Fetch<Log>().Single();

            logItem.Message.Should().Be(UniqueMessage);
            VerifyLog(logItem, LogLevel.Info);
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

            var logItem = Db.Fetch<Log>().Single();

            logItem.Message.Should().HaveLength(message.Length);
            logItem.Message.Should().Be(message);
            VerifyLog(logItem, LogLevel.Info);
        }


        [Test]
        public void clearLog()
        {
            //Act
            for (int i = 0; i < 10; i++)
            {
                Logger.Info(UniqueMessage);
            }

            //Assert
            var provider = new LogProvider(Db, dbContext);
            provider.GetAllLogs().Should().HaveCount(10);
            provider.DeleteAll();
            provider.GetAllLogs().Should().HaveCount(1);
        }

        [Test]
        public void write_log_exception()
        {
            var ex = new InvalidOperationException("Fake Exception");
            //Act

            Logger.ErrorException(UniqueMessage, ex);

            //Assert
            var logItem = Db.Fetch<Log>().Single();

            VerifyLog(logItem, LogLevel.Error);
            logItem.Message.Should().Be(UniqueMessage + ": " + ex.Message);
            logItem.ExceptionType.Should().Be(ex.GetType().ToString());
            logItem.Exception.Should().Be(ex.ToString());

            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void exception_log_with_no_message_should_use_exceptions_message()
        {

            var ex = new InvalidOperationException("Fake Exception");
            UniqueMessage = String.Empty;


            Logger.ErrorException(UniqueMessage, ex);

            var logItem = Db.Fetch<Log>().Single();


            logItem.Message.Should().Be(ex.Message);

            VerifyLog(logItem, LogLevel.Error);

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
        public void Trim_Logs_should_clear_logs_older_than_30_days()
        {
            //Setup
            var historyItem = Builder<Log>.CreateListOfSize(30)
                .TheFirst(20).With(c => c.Time = DateTime.Now)
                .TheNext(10).With(c => c.Time = DateTime.Now.AddDays(-31))
                .Build();

            Db.InsertMany(historyItem);

            //Act
            Db.Fetch<Log>().Should().HaveCount(30);
            Mocker.Resolve<LogProvider>().Trim();

            //Assert
            var result = Db.Fetch<Log>();
            result.Should().HaveCount(20);
            result.Should().OnlyContain(s => s.Time > DateTime.Now.AddDays(-30));
        }

        private void VerifyLog(Log logItem, LogLevel level)
        {
            logItem.Time.Should().BeWithin(TimeSpan.FromSeconds(2));
            logItem.Logger.Should().Be(LOGGER_NAME);
            logItem.Level.Should().Be(level.Name);
            logItem.Method.Should().Be(new StackTrace().GetFrame(1).GetMethod().Name);
            Logger.Name.Should().EndWith(logItem.Logger);
        }
    }
}
