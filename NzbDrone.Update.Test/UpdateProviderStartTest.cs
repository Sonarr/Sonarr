using System.IO;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderStartTest
    {
        AutoMoqer mocker = new AutoMoqer();

        private const string UPDATE_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone\";
        private const string BACKUP_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone_backup\";
        private const string TARGET_FOLDER = @"C:\NzbDrone\";

        Mock<EnviromentProvider> _enviromentProvider;


        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();
            
            _enviromentProvider = mocker.GetMock<EnviromentProvider>();

            _enviromentProvider.SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

            mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(UPDATE_FOLDER))
               .Returns(true);

            mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(TARGET_FOLDER))
               .Returns(true);
        }

        public void WithInstalledService()
        {
            mocker.GetMock<ServiceProvider>()
              .Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
              .Returns(true);
        }

        [Test]
        public void should_stop_nzbdrone_service_if_installed()
        {
            WithInstalledService();

            //Act
            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_kill_nzbdrone_process_if_running()
        {
            var proccesses = Builder<ProcessInfo>.CreateListOfSize(2).Build();

            mocker.GetMock<ProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProccessName))
                .Returns(proccesses);

            //Act
            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[0].Id), Times.Once());
            mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[1].Id), Times.Once());
            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_create_backup_of_current_installation()
        {
            var diskprovider = mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(TARGET_FOLDER, BACKUP_FOLDER));

            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_copy_update_package_to_target()
        {
            var diskprovider = mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER));

            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_restore_if_update_fails()
        {
            var diskprovider = mocker.GetMock<DiskProvider>();
            diskprovider.Setup(c => c.CopyDirectory(BACKUP_FOLDER, TARGET_FOLDER));

            diskprovider.Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER)).Throws(new IOException());

            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_restart_service_if_service_was_running()
        {
            WithInstalledService();

            var serviceProvider = mocker.GetMock<ServiceProvider>();

            serviceProvider.Setup(c => c.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(true);

            //Act
            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            serviceProvider
                .Verify(c => c.Start(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());

            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_not_restart_service_if_service_was_not_running()
        {
            WithInstalledService();

            var serviceProvider = mocker.GetMock<ServiceProvider>();

            serviceProvider.Setup(c => c.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
                .Returns(false);

            //Act
            mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            mocker.GetMock<ProcessProvider>()
                .Verify(c => c.Start(TARGET_FOLDER + "nzbdrone.exe"), Times.Once());

            serviceProvider
                .Verify(c => c.Start(It.IsAny<string>()), Times.Never());

            mocker.VerifyAllMocks();
        }
    }
}
