using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class DownloadClientStatusRepositoryFixture : DbTest<DownloadClientStatusRepository, DownloadClientStatus>
    {
        [Test]
        public void should_persist_submission_pause()
        {
            var failedAt = DateTime.UtcNow.AddDays(-1);
            Subject.Insert(new DownloadClientStatus
            {
                ProviderId = 1,
                SubmissionFailureCount = 3,
                MostRecentSubmissionFailure = failedAt
            });

            var status = Mocker.Resolve<DownloadClientStatusRepository>().FindByProviderId(1);

            status.SubmissionFailureCount.Should().Be(3);
            status.MostRecentSubmissionFailure.Should().BeCloseTo(failedAt, TimeSpan.FromSeconds(1));
            status.IsSubmissionPaused().Should().BeTrue();
            status.IsDisabled().Should().BeTrue();
        }
    }
}
