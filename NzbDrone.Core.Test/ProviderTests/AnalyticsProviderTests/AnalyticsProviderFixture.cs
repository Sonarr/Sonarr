using System.Linq;
using DeskMetrics;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.AnalyticsProviderTests
{
    // ReSharper disable InconsistentNaming
    public class AnalyticsProviderFixture : CoreTest
    {
        [Test]
        public void checkpoint_should_stop_existing_start_then_start_again()
        {
            Mocker.GetMock<IDeskMetricsClient>().SetupGet(c => c.Started).Returns(true);
            var provider = Mocker.Resolve<AnalyticsProvider>();

            provider.Checkpoint();

            Mocker.GetMock<IDeskMetricsClient>().Verify(c=>c.Start(), Times.Once());
            Mocker.GetMock<IDeskMetricsClient>().Verify(c=>c.Stop(), Times.Once());
        }

        [Test]
        public void checkpoint_should_not_stop_existing_if_not_started()
        {
            Mocker.GetMock<IDeskMetricsClient>().SetupGet(c => c.Started).Returns(false);
            var provider = Mocker.Resolve<AnalyticsProvider>();

            provider.Checkpoint();

            Mocker.GetMock<IDeskMetricsClient>().Verify(c => c.Start(), Times.Once());
            Mocker.GetMock<IDeskMetricsClient>().Verify(c => c.Stop(), Times.Never());
        }

        [Test]
        public void new_install_should_be_registered()
        {
            EnviromentProvider.IsNewInstall = true;

            var provider = Mocker.Resolve<AnalyticsProvider>();

            provider.Checkpoint();

            Mocker.GetMock<IDeskMetricsClient>().Verify(c => c.RegisterInstall(), Times.Once());
        }

        [Test]
        public void upgrade_should_not_register_install()
        {
            EnviromentProvider.IsNewInstall = false;

            var provider = Mocker.Resolve<AnalyticsProvider>();

            provider.Checkpoint();

            Mocker.GetMock<IDeskMetricsClient>().Verify(c => c.RegisterInstall(), Times.Never());
        }

        
    }
}
