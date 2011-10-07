using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ServiceControllerTests
    {
        [Test]
        public void Exists_should_find_spooler_service()
        {
            var serviceController = new ServiceProvider();

            //Act
            var exists = serviceController.ServiceExist("spooler");

            exists.Should().BeTrue();
        }

        [Test]
        public void Exists_should_not_find_random_service()
        {
            var serviceController = new ServiceProvider();

            //Act
            var exists = serviceController.ServiceExist("random_service_name");

            exists.Should().BeFalse();
        }


        [Test]
        public void Service_should_be_installed_and_then_uninstalled()
        {
            var serviceController = new ServiceProvider();

            //Act
            serviceController.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeFalse();
            serviceController.Install();
            serviceController.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeTrue();
            serviceController.UnInstall();
            serviceController.ServiceExist(ServiceProvider.NzbDroneServiceName).Should().BeFalse();
        }
    }
}
