using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.InstrumentationTests
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
        public async Task write_log()
        {
            _logger.Info(_uniqueMessage);

            Thread.Sleep(1000);

            var storedModel = await GetStoredModelAsync();
            storedModel.Message.Should().Be(_uniqueMessage);
            VerifyLog(storedModel, LogLevel.Info);
        }

        [Test]
        public async Task write_long_log()
        {
            var message = string.Empty;
            for (var i = 0; i < 100; i++)
            {
                message += Guid.NewGuid();
            }

            _logger.Info(message);

            Thread.Sleep(1000);

            var storedModel = await GetStoredModelAsync();
            storedModel.Message.Should().HaveLength(message.Length);
            storedModel.Message.Should().Be(message);
            VerifyLog(storedModel, LogLevel.Info);
        }

        [Test]
        public async Task write_log_exception()
        {
            var ex = new InvalidOperationException("Fake Exception");

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(1000);

            var storedModel = await GetStoredModelAsync();
            VerifyLog(storedModel, LogLevel.Error);
            storedModel.Message.Should().Be(_uniqueMessage + ": " + ex.Message);
            storedModel.ExceptionType.Should().Be(ex.GetType().ToString());
            storedModel.Exception.Should().Be(ex.ToString());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public async Task exception_log_with_no_message_should_use_exceptions_message()
        {
            var ex = new InvalidOperationException("Fake Exception");
            _uniqueMessage = string.Empty;

            _logger.Error(ex, _uniqueMessage);

            Thread.Sleep(1000);

            var storedModel = await GetStoredModelAsync();
            storedModel.Message.Should().Be(ex.Message);

            VerifyLog(storedModel, LogLevel.Error);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void null_string_as_arg_should_not_fail()
        {
            var epFile = new EpisodeFile();
            _logger.Debug("File {0} no longer exists on disk. removing from database.", epFile.RelativePath);

            Thread.Sleep(1000);

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
            logItem.Logger.Should().Be(GetType().Name);
            logItem.Level.Should().Be(level.Name);
            _logger.Name.Should().EndWith(logItem.Logger);
        }
    }
}
