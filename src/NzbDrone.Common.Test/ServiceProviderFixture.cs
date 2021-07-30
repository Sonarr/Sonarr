using System;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ServiceProviderFixture : TestBase<ServiceProvider>
    {
        private const string ALWAYS_INSTALLED_SERVICE = "SCardSvr"; //Smart Card
        private const string TEMP_SERVICE_NAME = "NzbDrone_Nunit";

        [SetUp]
        public void Setup()
        {
            WindowsOnly();

            Mocker.SetConstant<IProcessProvider>(Mocker.Resolve<ProcessProvider>());

            CleanupService();
        }

        [TearDown]
        public void TearDown()
        {
            if (OsInfo.IsWindows)
            {
                CleanupService();
            }
        }


        private void CleanupService()
        {
            if (Subject.ServiceExist(TEMP_SERVICE_NAME))
            {
                Subject.Uninstall(TEMP_SERVICE_NAME);
            }

            if (Subject.IsServiceRunning(ALWAYS_INSTALLED_SERVICE))
            {
                Subject.Stop(ALWAYS_INSTALLED_SERVICE);
            }
        }

        [Test]
        public void Exists_should_find_existing_service()
        {
            Subject.ServiceExist(ALWAYS_INSTALLED_SERVICE).Should().BeTrue();
        }

        [Test]
        public void Exists_should_not_find_random_service()
        {
            Subject.ServiceExist("random_service_name").Should().BeFalse();
        }


        [Test]
        public void Service_should_be_installed_and_then_uninstalled()
        {
            if (!IsAnAdministrator())
            {
                Assert.Inconclusive("Can't run test without Administrator rights");
            }

            Subject.ServiceExist(TEMP_SERVICE_NAME).Should().BeFalse("Service already installed");
            Subject.Install(TEMP_SERVICE_NAME);
            Subject.ServiceExist(TEMP_SERVICE_NAME).Should().BeTrue();
            Subject.Uninstall(TEMP_SERVICE_NAME);
            Thread.Sleep(2000);
            Subject.ServiceExist(TEMP_SERVICE_NAME).Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        [Explicit]
        [ManualTest]
        public void UnInstallService()
        {
            Subject.Uninstall(ServiceProvider.SERVICE_NAME);
            Subject.ServiceExist(ServiceProvider.SERVICE_NAME).Should().BeFalse();
        }

        [Test]
        [Explicit]
        [ManualTest]
        public void Should_be_able_to_start_and_stop_service()
        {
            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().NotBe(ServiceControllerStatus.Running);

            Subject.Start(ALWAYS_INSTALLED_SERVICE);

            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().Be(ServiceControllerStatus.Running);

            Subject.Stop(ALWAYS_INSTALLED_SERVICE);

            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().Be(ServiceControllerStatus.Stopped);
        }

        [Test]
        public void should_throw_if_starting_a_running_service()
        {
            if (!IsAnAdministrator())
            {
                Assert.Inconclusive("Can't run test without Administrator rights");
            }

            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
               .Should().NotBe(ServiceControllerStatus.Running);

            Subject.Start(ALWAYS_INSTALLED_SERVICE);
            Assert.Throws<InvalidOperationException>(() => Subject.Start(ALWAYS_INSTALLED_SERVICE));


            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void Should_log_warn_if_on_stop_if_service_is_already_stopped()
        {
            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().NotBe(ServiceControllerStatus.Running);


            Subject.Stop(ALWAYS_INSTALLED_SERVICE);


            Subject.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().Be(ServiceControllerStatus.Stopped);

            ExceptionVerification.ExpectedWarns(1);
        }
        private static bool IsAnAdministrator()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
