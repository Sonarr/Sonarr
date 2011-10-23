// ReSharper disable InconsistentNaming

using System.ServiceProcess;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ServiceControllerTests
    {
        private const string ALWAYS_INSTALLED_SERVICE = "SCardSvr"; //Smart Card
        private ServiceProvider serviceProvider;


        [SetUp]
        public void Setup()
        {
            serviceProvider = new ServiceProvider();
        }

        [Test]
        public void Exists_should_find_existing_service()
        {
            //Act
            var exists = serviceProvider.ServiceExist(ALWAYS_INSTALLED_SERVICE);

            exists.Should().BeTrue();
        }

        [Test]
        public void Exists_should_not_find_random_service()
        {
            //Act
            var exists = serviceProvider.ServiceExist("random_service_name");

            exists.Should().BeFalse();
        }


        [Test]
        public void Service_should_be_installed_and_then_uninstalled()
        {
            //Act
            serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeFalse("Service already installed");
            serviceProvider.Install();
            serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeTrue();
            serviceProvider.UnInstall();
            serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeFalse();
        }

        [Test]
        [Explicit]
        public void UnInstallService()
        {
            //Act
            serviceProvider.UnInstall();
            serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeFalse();
        }

        [Test]
        [Timeout(10000)]
        public void Should_be_able_to_start_and_stop_service()
        {
            serviceProvider.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().NotBe(ServiceControllerStatus.Running);

            serviceProvider.Start(ALWAYS_INSTALLED_SERVICE);

            serviceProvider.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().Be(ServiceControllerStatus.Running);

            serviceProvider.Stop(ALWAYS_INSTALLED_SERVICE);

            serviceProvider.GetService(ALWAYS_INSTALLED_SERVICE).Status
                .Should().Be(ServiceControllerStatus.Stopped);
        }
    }
}
