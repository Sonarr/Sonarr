using System.ServiceProcess;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Model;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class RouterTest : TestBase
    {

        [TestCase(null, ApplicationMode.Console)]
        [TestCase("", ApplicationMode.Console)]
        [TestCase("1", ApplicationMode.Help)]
        [TestCase("ii", ApplicationMode.Help)]
        [TestCase("uu", ApplicationMode.Help)]
        [TestCase("i", ApplicationMode.InstallService)]
        [TestCase("I", ApplicationMode.InstallService)]
        [TestCase("/I", ApplicationMode.InstallService)]
        [TestCase("/i", ApplicationMode.InstallService)]
        [TestCase("-I", ApplicationMode.InstallService)]
        [TestCase("-i", ApplicationMode.InstallService)]
        [TestCase("u", ApplicationMode.UninstallService)]
        [TestCase("U", ApplicationMode.UninstallService)]
        [TestCase("/U", ApplicationMode.UninstallService)]
        [TestCase("/u", ApplicationMode.UninstallService)]
        [TestCase("-U", ApplicationMode.UninstallService)]
        [TestCase("-u", ApplicationMode.UninstallService)]
        public void GetApplicationMode_single_arg(string arg, ApplicationMode mode)
        {
            Router.GetApplicationMode(new[] { arg }).Should().Be(mode);
        }

        [TestCase("", "", ApplicationMode.Console)]
        [TestCase("", null, ApplicationMode.Console)]
        [TestCase("i", "n", ApplicationMode.Help)]
        public void GetApplicationMode_two_args(string a, string b, ApplicationMode mode)
        {
            Router.GetApplicationMode(new[] { a, b }).Should().Be(mode);
        }

        [Test]
        public void Route_should_call_install_service_when_application_mode_is_install()
        {
            WithStrictMocker();
            var serviceProviderMock = Mocker.GetMock<ServiceProvider>();
            serviceProviderMock.Setup(c => c.Install(ServiceProvider.NZBDRONE_SERVICE_NAME));
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(false);
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationMode.InstallService);

            serviceProviderMock.Verify(c => c.Install(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
        }

        
        [Test]
        public void Route_should_call_uninstall_service_when_application_mode_is_uninstall()
        {
            WithStrictMocker();
            var serviceProviderMock = Mocker.GetMock<ServiceProvider>();
            serviceProviderMock.Setup(c => c.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME));
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);
            serviceProviderMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationMode.UninstallService);

            serviceProviderMock.Verify(c => c.UnInstall(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
        }

        [Test]
        public void Route_should_call_console_service_when_application_mode_is_console()
        {
            WithStrictMocker();
            var consoleProvider = Mocker.GetMock<ConsoleProvider>();
            var appServerProvider = Mocker.GetMock<ApplicationServer>();
            consoleProvider.Setup(c => c.WaitForClose());
            appServerProvider.Setup(c => c.Start());
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationMode.Console);

            consoleProvider.Verify(c => c.WaitForClose(), Times.Once());
            appServerProvider.Verify(c => c.Start(), Times.Once());
        }

        [TestCase(ApplicationMode.Console)]
        [TestCase(ApplicationMode.InstallService)]
        [TestCase(ApplicationMode.UninstallService)]
        [TestCase(ApplicationMode.Help)]
        public void Route_should_call_service_start_when_run_in_service_more(ApplicationMode applicationMode)
        {
            WithStrictMocker();
            var envMock = Mocker.GetMock<EnviromentProvider>();
            var serviceProvider = Mocker.GetMock<ServiceProvider>();

            envMock.SetupGet(c => c.IsUserInteractive).Returns(false);

            serviceProvider.Setup(c => c.Run(It.IsAny<ServiceBase>()));

            Mocker.Resolve<Router>().Route(applicationMode);

            serviceProvider.Verify(c => c.Run(It.IsAny<ServiceBase>()), Times.Once());
        }


        [Test]
        public void show_error_on_install_if_service_already_exist()
        {
            WithStrictMocker();
            var consoleMock = Mocker.GetMock<ConsoleProvider>();
            var serviceMock = Mocker.GetMock<ServiceProvider>();
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceAlreadyExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(true);

            Mocker.Resolve<Router>().Route(ApplicationMode.InstallService);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void show_error_on_uninstall_if_service_doesnt_exist()
        {
            WithStrictMocker();
            var consoleMock = Mocker.GetMock<ConsoleProvider>();
            var serviceMock = Mocker.GetMock<ServiceProvider>();
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.IsUserInteractive).Returns(true);

            consoleMock.Setup(c => c.PrintServiceDoestExist());
            serviceMock.Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(false);

            Mocker.Resolve<Router>().Route(ApplicationMode.UninstallService);

            Mocker.VerifyAllMocks();
        }
    }
}
