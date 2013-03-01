using System.ServiceProcess;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class RouterTest : TestBase
    {

        [TestCase(null, ApplicationModes.Console)]
        [TestCase("", ApplicationModes.Console)]
        [TestCase("1", ApplicationModes.Help)]
        [TestCase("ii", ApplicationModes.Help)]
        [TestCase("uu", ApplicationModes.Help)]
        [TestCase("i", ApplicationModes.InstallService)]
        [TestCase("I", ApplicationModes.InstallService)]
        [TestCase("/I", ApplicationModes.InstallService)]
        [TestCase("/i", ApplicationModes.InstallService)]
        [TestCase("-I", ApplicationModes.InstallService)]
        [TestCase("-i", ApplicationModes.InstallService)]
        [TestCase("u", ApplicationModes.UninstallService)]
        [TestCase("U", ApplicationModes.UninstallService)]
        [TestCase("/U", ApplicationModes.UninstallService)]
        [TestCase("/u", ApplicationModes.UninstallService)]
        [TestCase("-U", ApplicationModes.UninstallService)]
        [TestCase("-u", ApplicationModes.UninstallService)]
        public void GetApplicationMode_single_arg(string arg, ApplicationModes modes)
        {
            Router.GetApplicationMode(new[] { arg }).Should().Be(modes);
        }

        [TestCase("", "", ApplicationModes.Console)]
        [TestCase("", null, ApplicationModes.Console)]
        [TestCase("i", "n", ApplicationModes.Help)]
        public void GetApplicationMode_two_args(string a, string b, ApplicationModes modes)
        {
            Router.GetApplicationMode(new[] { a, b }).Should().Be(modes);
        }

        [Test]
        public void Route_should_call_install_service_when_application_mode_is_install()
        {
            var serviceProviderMock = Mocker.GetMock<ServiceProvider>(MockBehavior.Strict);
            serviceProviderMock.Setup(c => c.Install(ServiceProvider.NZBDRONE_SERVICE_NAME));
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(false);
            serviceProviderMock.Setup(c => c.Start(ServiceProvider.NZBDRONE_SERVICE_NAME));
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationModes.InstallService);

            serviceProviderMock.Verify(c => c.Install(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
        }

        
        [Test]
        public void Route_should_call_uninstall_service_when_application_mode_is_uninstall()
        {
            var serviceProviderMock = Mocker.GetMock<ServiceProvider>();
            serviceProviderMock.Setup(c => c.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME));
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationModes.UninstallService);

            serviceProviderMock.Verify(c => c.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
        }

        [Test]
        public void Route_should_call_console_service_when_application_mode_is_console()
        {
            var consoleProvider = Mocker.GetMock<ConsoleProvider>();
            var appServerProvider = Mocker.GetMock<ApplicationServer>();
            consoleProvider.Setup(c => c.WaitForClose());
            appServerProvider.Setup(c => c.Start());
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationModes.Console);

            consoleProvider.Verify(c => c.WaitForClose(), Times.Once());
            appServerProvider.Verify(c => c.Start(), Times.Once());
        }

        [TestCase(ApplicationModes.Console)]
        [TestCase(ApplicationModes.InstallService)]
        [TestCase(ApplicationModes.UninstallService)]
        [TestCase(ApplicationModes.Help)]
        public void Route_should_call_service_start_when_run_in_service_more(ApplicationModes applicationModes)
        {
            var envMock = Mocker.GetMock<EnvironmentProvider>();
            var serviceProvider = Mocker.GetMock<ServiceProvider>();

            envMock.SetupGet(c => c.IsUserInteractive).Returns(false);

            serviceProvider.Setup(c => c.Run(It.IsAny<ServiceBase>()));

            Mocker.Resolve<Router>().Route(applicationModes);

            serviceProvider.Verify(c => c.Run(It.IsAny<ServiceBase>()), Times.Once());
        }


        [Test]
        public void show_error_on_install_if_service_already_exist()
        {
            var consoleMock = Mocker.GetMock<ConsoleProvider>();
            var serviceMock = Mocker.GetMock<ServiceProvider>();
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceAlreadyExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationModes.InstallService);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void show_error_on_uninstall_if_service_doesnt_exist()
        {
            var consoleMock = Mocker.GetMock<ConsoleProvider>();
            var serviceMock = Mocker.GetMock<ServiceProvider>();
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceDoestExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(false);

            Mocker.Resolve<Router>().Route(ApplicationModes.UninstallService);

            Mocker.VerifyAllMocks();
        }
    }
}
