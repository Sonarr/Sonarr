using System;
using System.ServiceProcess;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    [Timeout(15000)]
    public class ServiceProviderTests : TestBase<ServiceProvider>
    {
        private const string ALWAYS_INSTALLED_SERVICE = "SCardSvr"; //Smart Card
        private const string TEMP_SERVICE_NAME = "NzbDrone_Nunit";

        [SetUp]
        public void Setup()
        {
            WindowsOnly();
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

            Subject.ServiceExist(TEMP_SERVICE_NAME).Should().BeFalse("Service already installed");
            Subject.Install(TEMP_SERVICE_NAME);
            Subject.ServiceExist(TEMP_SERVICE_NAME).Should().BeTrue();
            Subject.Uninstall(TEMP_SERVICE_NAME);
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
        public void should_throw_if_starting_a_running_serivce()
        {
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
    }
}
