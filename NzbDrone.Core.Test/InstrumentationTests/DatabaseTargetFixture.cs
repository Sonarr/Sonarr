using System;
using System.Diagnostics;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.InstrumentationTests
{
    [TestFixture]
    public class DatabaseTargetFixture : DbTest<DatabaseTarget, Log>
    {
        string _loggerName;

        private static string _uniqueMessage;
        Logger _logger;

        protected override MigrationType MigrationType
        {
            get
            {
                return MigrationType.Log;

            }
        }
        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<ILogRepository, LogRepository>();
            Mocker.Resolve<DatabaseTarget>().Register();

            LogManager.ReconfigExistingLoggers();


            _logger = LogManager.GetCurrentClassLogger();
            _loggerName = _logger.Name.Replace("NzbDrone.","");

            _uniqueMessage = "Unique message: " + Guid.NewGuid().ToString();
        }

        [Test]
        public void write_log()
        {
            _logger.Info(_uniqueMessage);

            StoredModel.Message.Should().Be(_uniqueMessage);
            VerifyLog(StoredModel, LogLevel.Info);
        }

        [Test]
        public void write_long_log()
        {
            var message = String.Empty;
            for (int i = 0; i < 100; i++)
            {
                message += Guid.NewGuid();
            }

            _logger.Info(message);

            StoredModel.Message.Should().HaveLength(message.Length);
            StoredModel.Message.Should().Be(message);
            VerifyLog(StoredModel, LogLevel.Info);
        }

        [Test]
        public void write_log_exception()
        {
            var ex = new InvalidOperationException("Fake Exception");

            _logger.ErrorException(_uniqueMessage, ex);


            VerifyLog(StoredModel, LogLevel.Error);
            StoredModel.Message.Should().Be(_uniqueMessage + ": " + ex.Message);
            StoredModel.ExceptionType.Should().Be(ex.GetType().ToString());
            StoredModel.Exception.Should().Be(ex.ToString());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void exception_log_with_no_message_should_use_exceptions_message()
        {

            var ex = new InvalidOperationException("Fake Exception");
            _uniqueMessage = String.Empty;


            _logger.ErrorException(_uniqueMessage, ex);

            StoredModel.Message.Should().Be(ex.Message);

            VerifyLog(StoredModel, LogLevel.Error);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            var epFile = new EpisodeFile();
            _logger.Trace("File {0} no longer exists on disk. removing from database.", epFile.Path);

            epFile.Path.Should().BeNull();
        }

        private void VerifyLog(Log logItem, LogLevel level)
        {
            logItem.Time.Should().BeWithin(TimeSpan.FromSeconds(2));
            logItem.Logger.Should().Be(_loggerName);
            logItem.Level.Should().Be(level.Name);
            logItem.Method.Should().Be(new StackTrace().GetFrame(1).GetMethod().Name);
            _logger.Name.Should().EndWith(logItem.Logger);
        }
    }
}
