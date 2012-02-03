using System.Linq;
using DeskMetrics;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.AnalyticsProviderTests
{
    // ReSharper disable InconsistentNaming
    public class AnalyticsProviderFixture : CoreTest
    {

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.UpdateUrl).Returns(UpdateProvider.DEFAULT_UPDATE_URL);
        }

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


        [Test]
        public void shouldnt_register_anything_if_not_on_master_branch()
        {
            EnviromentProvider.IsNewInstall = false;

            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.UpdateUrl).Returns("http://update.nzbdrone.com/master_auto/");

            Mocker.GetMock<IDeskMetricsClient>(MockBehavior.Strict);

            Mocker.Resolve<AnalyticsProvider>().Checkpoint();
        }

        [Test]
        public void new_install_shouldnt_register_anything_if_not_on_master_branch()
        {
            EnviromentProvider.IsNewInstall = true;

            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.UpdateUrl).Returns("http://update.nzbdrone.com/master_auto/");

            Mocker.GetMock<IDeskMetricsClient>(MockBehavior.Strict);

            Mocker.Resolve<AnalyticsProvider>().Checkpoint();
        }


        [TestCase("http://update.nzbdrone.com/master/")]
        [TestCase("http://update.nzbdrone.com/master//")]
        [TestCase("http://update.nzbdrone.com/master")]
        [TestCase("http://update.nzbdrone.com/master ")]
        [TestCase("http://update.nzbdrone.com/master/ ")]
        [TestCase("http://UPDATE.nzbdrone.COM/master/ ")]
        public void should_still_work_if_url_is_slightly_diffrent(string url)
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.UpdateUrl).Returns(url);

            var provider = Mocker.Resolve<AnalyticsProvider>();

            provider.Checkpoint();

            Mocker.GetMock<IDeskMetricsClient>().Verify(c => c.Start(), Times.Once());
        }

        
    }
}
