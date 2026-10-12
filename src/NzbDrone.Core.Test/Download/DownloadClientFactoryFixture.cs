using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class DownloadClientFactoryFixture : CoreTest<DownloadClientFactory>
    {
        private IDownloadClient _client;
        private DownloadClientStatus _status;

        [SetUp]
        public void Setup()
        {
            var client = new Mock<IDownloadClient>();
            var settings = new Mock<IProviderConfig>();
            settings.Setup(s => s.Validate()).Returns(new NzbDroneValidationResult());
            var definition = new DownloadClientDefinition
            {
                Id = 1,
                Name = "Test client",
                Enable = true,
                Implementation = client.Object.GetType().Name,
                Settings = settings.Object
            };
            client.SetupProperty(s => s.Definition, definition);
            _client = client.Object;
            _status = new DownloadClientStatus { ProviderId = 1 };

            Mocker.SetConstant<IEnumerable<IDownloadClient>>(new[] { _client });
            Mocker.GetMock<IServiceProvider>().Setup(s => s.GetService(_client.GetType())).Returns(_client);
            Mocker.GetMock<IDownloadClientRepository>().Setup(s => s.All()).Returns(new[] { definition });
            Mocker.GetMock<IDownloadClientStatusService>()
                .Setup(s => s.GetBlockedProviders())
                .Returns(new List<DownloadClientStatus> { _status });
        }

        [TestCase(1)]
        [TestCase(3)]
        public void should_keep_monitoring_after_submission_failures(int failures)
        {
            _status.SubmissionFailureCount = failures;
            _status.MostRecentSubmissionFailure = DateTime.UtcNow;

            Subject.DownloadHandlingEnabled().Should().Contain(_client);
        }

        [Test]
        public void should_keep_queue_failure_backoff_when_submissions_are_paused()
        {
            _status.SubmissionFailureCount = 3;
            _status.DisabledTill = DateTime.UtcNow.AddMinutes(5);

            Subject.DownloadHandlingEnabled().Should().BeEmpty();
        }

        [Test]
        public void should_resume_monitoring_when_queue_failure_backoff_expires()
        {
            _status.SubmissionFailureCount = 3;
            _status.DisabledTill = DateTime.UtcNow.AddMinutes(-1);

            Subject.DownloadHandlingEnabled().Should().Contain(_client);
        }
    }
}
