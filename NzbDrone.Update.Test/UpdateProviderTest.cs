using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMoq;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderTest
    {
        AutoMoqer mocker = new AutoMoqer();

        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();

            mocker.GetMock<EnviromentProvider>()
                .Setup(c => c.StartUpPath).Returns(@"C:\Temp\NzbDrone_update\");
        }


        [Test]
        public void update_should_abort_with_message_if_update_package_isnt_in_current_folder()
        {
           string sandboxFolder = @"C:\Temp\NzbDrone_update\nzbdrone_update";
            mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(sandboxFolder)).
                Returns(false);

            mocker.GetMock<ServiceProvider>(MockBehavior.Strict);

            //Act
            mocker.Resolve<UpdateProvider>().Start();

            //Assert
            mocker.GetMock<ConsoleProvider>().Verify(c => c.UpdateFolderDoestExist(sandboxFolder), Times.Once());
            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_stop_nzbdrone_service_if_installed()
        {
            mocker.GetMock<DiskProvider>()
                 .Setup(c => c.FolderExists(It.IsAny<string>())).
                 Returns(true);

            mocker.GetMock<ServiceProvider>()
                .Setup(c => c.ServiceExist(ServiceProvider.NzbDroneServiceName))
                .Returns(true);
            
            //Act
            mocker.Resolve<UpdateProvider>().Start();

            //Assert
            mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(ServiceProvider.NzbDroneServiceName), Times.Once());
            mocker.VerifyAllMocks();
        }
    }
}
