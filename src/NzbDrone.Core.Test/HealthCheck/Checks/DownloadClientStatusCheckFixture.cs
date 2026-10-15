using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientStatusCheckFixture : CoreTest<DownloadClientStatusCheck>
    {
        private List<IDownloadClient> _clients;
        private List<DownloadClientStatus> _blockedClients;

        [SetUp]
        public void Setup()
        {
            var client = new Mock<IDownloadClient>();
            client.SetupGet(s => s.Definition).Returns(new DownloadClientDefinition { Id = 1, Name = "Paused client" });
            _clients = new List<IDownloadClient> { client.Object };
            _blockedClients = new List<DownloadClientStatus>();

            Mocker.GetMock<IDownloadClientFactory>().Setup(s => s.GetAvailableProviders()).Returns(_clients);
            Mocker.GetMock<IDownloadClientStatusService>().Setup(s => s.GetBlockedProviders()).Returns(_blockedClients);
            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .Returns("Some Warning Message");
        }

        [TestCase(false, HealthCheckResult.Error)]
        [TestCase(true, HealthCheckResult.Warning)]
        public void should_show_submission_pause_when_clients_are_paused(bool healthyClient, HealthCheckResult result)
        {
            _blockedClients.Add(new DownloadClientStatus { ProviderId = 1, SubmissionFailureCount = 3 });

            if (healthyClient)
            {
                var client = new Mock<IDownloadClient>();
                client.SetupGet(s => s.Definition).Returns(new DownloadClientDefinition { Id = 2, Name = "Healthy client" });
                _clients.Add(client.Object);
            }

            var check = Subject.Check();

            check.Type.Should().Be(result);
            check.Reason.Should().Be(HealthCheckReason.DownloadClientStatusSubmissionPaused);
            Mocker.GetMock<ILocalizationService>()
                .Verify(s => s.GetLocalizedString("DownloadClientStatusSubmissionPausedHealthCheckMessage", It.Is<Dictionary<string, object>>(d => (string)d["downloadClientNames"] == "Paused client")), Times.Once());
        }

        [Test]
        public void should_clear_warning_when_no_clients_are_blocked()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_not_report_submission_pause_for_queue_failures()
        {
            _blockedClients.Add(new DownloadClientStatus { ProviderId = 1, EscalationLevel = 3 });

            var check = Subject.Check();

            check.ShouldBeError();
            check.Reason.Should().Be(HealthCheckReason.DownloadClientStatusAllClients);
        }
    }
}
