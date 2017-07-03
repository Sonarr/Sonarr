using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ImportMechanismCheckFixture : CoreTest<ImportMechanismCheck>
    {
        private void GivenCompletedDownloadHandling(bool? enabled = null)
        {
            if (enabled.HasValue)
            {
                Mocker.GetMock<IConfigService>()
                      .Setup(s => s.IsDefined("EnableCompletedDownloadHandling"))
                      .Returns(true);

                Mocker.GetMock<IConfigService>()
                      .SetupGet(s => s.EnableCompletedDownloadHandling)
                      .Returns(enabled.Value);
            }
        }

        [Test]
        public void should_return_warning_when_completed_download_handling_not_configured()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_both_completeddownloadhandling_and_dronefactory_are_not_configured()
        {
            GivenCompletedDownloadHandling(false);
            
            Subject.Check().ShouldBeWarning();
        }
        
        [Test]
        public void should_return_ok_when_no_issues_found()
        {
            GivenCompletedDownloadHandling(true);

            Subject.Check().ShouldBeOk();
        }
    }
}
