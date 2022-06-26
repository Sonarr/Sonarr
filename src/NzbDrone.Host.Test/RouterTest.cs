using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Host;
using NzbDrone.Test.Common;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class RouterTest : TestBase<UtilityModeRouter>
    {
        [SetUp]
        public void Setup()
        {
            WindowsOnly();
        }

        [Test]
        public void Route_should_call_install_service_when_application_mode_is_install()
        {
            var serviceProviderMock = Mocker.GetMock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.SERVICE_NAME)).Returns(false);
            serviceProviderMock.Setup(c => c.Install(ServiceProvider.SERVICE_NAME));
            serviceProviderMock.Setup(c => c.SetPermissions(ServiceProvider.SERVICE_NAME));

            Mocker.GetMock<IProcessProvider>()
                  .Setup(c => c.SpawnNewProcess("sc.exe", It.IsAny<string>(), null, true));

            Mocker.GetMock<IRuntimeInfo>().SetupGet(c => c.IsUserInteractive).Returns(true);

            Subject.Route(ApplicationModes.InstallService);

            serviceProviderMock.Verify(c => c.Install(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void Route_should_call_uninstall_service_when_application_mode_is_uninstall()
        {
            var serviceProviderMock = Mocker.GetMock<IServiceProvider>();
            serviceProviderMock.Setup(c => c.Uninstall(ServiceProvider.SERVICE_NAME));
            Mocker.GetMock<IRuntimeInfo>().SetupGet(c => c.IsUserInteractive).Returns(true);
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.SERVICE_NAME)).Returns(true);

            Subject.Route(ApplicationModes.UninstallService);

            serviceProviderMock.Verify(c => c.Uninstall(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void show_error_on_install_if_service_already_exist()
        {
            var consoleMock = Mocker.GetMock<IConsoleService>();
            var serviceMock = Mocker.GetMock<IServiceProvider>();
            Mocker.GetMock<IRuntimeInfo>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceAlreadyExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.SERVICE_NAME)).Returns(true);

            Subject.Route(ApplicationModes.InstallService);
        }

        [Test]
        public void show_error_on_uninstall_if_service_doesnt_exist()
        {
            var consoleMock = Mocker.GetMock<IConsoleService>();
            var serviceMock = Mocker.GetMock<IServiceProvider>();
            Mocker.GetMock<IRuntimeInfo>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceDoesNotExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.SERVICE_NAME)).Returns(false);

            Subject.Route(ApplicationModes.UninstallService);
        }
    }
}
