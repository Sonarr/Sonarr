using System;
using System.Threading;
using FluentAssertions;
using Marr.Data;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.Instrumentation
{
    [TestFixture]
    public class DatabaseTargetFixture : DbTest<DatabaseTarget, Log>
    {
        private static string _uniqueMessage;
        private Logger _logger;

        protected override MigrationType MigrationType => MigrationType.Log;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<ILogRepository, LogRepository>();
            Mocker.Resolve<DatabaseTarget>().Register();

            LogManager.ReconfigExistingLoggers();

            _logger = NzbDroneLogger.GetLogger(this);

            _uniqueMessage = "Unique message: " + Guid.NewGuid();
        }

        [Test]
        public void write_log()
        {
            _logger.Info(_uniqueMessage);

            Thread.Sleep(600);

            StoredModel.Message.Should().Be(_uniqueMessage);
            VerifyLog(StoredModel, LogLevel.Info);
        }

        [Test]
        public void write_long_log()
        {
            var message = string.Empty;
            for (int i = 0; i < 100; i++)
            {
                message += Guid.NewGuid();
            }

            _logger.Info(message);

            Thread.Sleep(600);

            StoredModel.Message.Should().HaveLength(message.Length);
            StoredModel.Message.Should().Be(message);
            VerifyLog(StoredModel, LogLevel.Info);
        }

        [Test]
        [Explicit]
        [ManualTest]
        public void perf_test()
        {
            MapRepository.Instance.EnableTraceLogging = false;
            for (int i = 0; i < 1000; i++)
            {
                _logger.Info(Guid.NewGuid());
            }

            Thread.Sleep(600);

            MapRepository.Instance.EnableTraceLogging = true;
        }

        [Test]
        public void write_log_exception()
        {
            var ex = new InvalidOperationException("Fake Exception");

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(600);

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
            _uniqueMessage = string.Empty;

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(600);

            StoredModel.Message.Should().Be(ex.Message);

            VerifyLog(StoredModel, LogLevel.Error);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            var epFile = new EpisodeFile();
            _logger.Debug("File {0} no longer exists on disk. removing from database.", epFile.RelativePath);

            Thread.Sleep(600);

            epFile.RelativePath.Should().BeNull();
        }

        [TearDown]
        public void Teardown()
        {
            Mocker.Resolve<DatabaseTarget>().UnRegister();
        }

        private void VerifyLog(Log logItem, LogLevel level)
        {
            logItem.Time.Should().BeWithin(TimeSpan.FromSeconds(2));
            logItem.Logger.Should().Be(this.GetType().Name);
            logItem.Level.Should().Be(level.Name);
            _logger.Name.Should().EndWith(logItem.Logger);
        }
    }
}
