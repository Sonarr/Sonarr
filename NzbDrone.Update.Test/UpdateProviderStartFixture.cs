using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderStartFixture : TestBase
    {
        private const string UPDATE_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone\";
        private const string BACKUP_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone_backup\";
        private const string SANDBOX_LOG_FOLDER = @"C:\Temp\nzbdrone_update\UpdateLogs\";
        private const string TARGET_FOLDER = @"C:\NzbDrone\";
        private const string UPDATE_LOG_FOLDER = @"C:\NzbDrone\UpdateLogs\";

        Mock<EnvironmentProvider> _environmentProvider;


        [SetUp]
        public void Setup()
        {

            _environmentProvider = Mocker.GetMock<EnvironmentProvider>();

            _environmentProvider.SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

            Mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(UPDATE_FOLDER))
               .Returns(true);

            Mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(TARGET_FOLDER))
               .Returns(true);
        }

        private void WithInstalledService()
        {
            Mocker.GetMock<ServiceProvider>()
              .Setup(c => c.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
              .Returns(true);
        }

        private void WithServiceRunning(bool state)
        {
            Mocker.GetMock<ServiceProvider>()
                .Setup(c => c.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME)).Returns(state);
        }

        [Test]
        public void should_stop_nzbdrone_service_if_installed_and_running()
        {
            WithInstalledService();
            WithServiceRunning(true);

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());
        }

        [Test]
        public void should_not_stop_nzbdrone_service_if_installed_but_not_running()
        {
            WithInstalledService();
            WithServiceRunning(false);

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Never());
        }

        [Test]
        public void should_not_stop_nzbdrone_service_if_service_isnt_installed()
        {
            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<ServiceProvider>().Verify(c => c.Stop(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_kill_nzbdrone_process_if_running()
        {
            var proccesses = Builder<ProcessInfo>.CreateListOfSize(2).Build().ToList();

            Mocker.GetMock<ProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProccessName))
                .Returns(proccesses);

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[0].Id), Times.Once());
            Mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(proccesses[1].Id), Times.Once());
        }

        [Test]
        public void should_not_kill_nzbdrone_process_not_running()
        {
            Mocker.GetMock<ProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProccessName))
                .Returns(new List<ProcessInfo>());

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<ProcessProvider>().Verify(c => c.Kill(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_stop_orphan_iisexpress_instances()
        {
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<IHostController>().Verify(c => c.StopServer(), Times.Once());
        }

        [Test]
        public void should_create_backup_of_current_installation()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(TARGET_FOLDER, BACKUP_FOLDER));

            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);
        }

        [Test]
        public void should_copy_update_package_to_target()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER));

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.DeleteFolder(UPDATE_FOLDER, true));

            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);
        }

        [Test]
        public void should_restore_if_update_fails()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            Mocker.GetMock<DiskProvider>()
                .Verify(c => c.CopyDirectory(BACKUP_FOLDER, TARGET_FOLDER), Times.Once());
            ExceptionVerification.ExpectedFatals(1);
        }

        [Test]
        public void should_restart_service_if_service_was_running()
        {
            WithInstalledService();
            WithServiceRunning(true);

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            VerifyServiceRestart();
        }

        [Test]
        public void should_restart_process_if_service_was_not_running()
        {
            WithInstalledService();
            WithServiceRunning(false);

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            VerifyProcessRestart();
        }

        [Test]
        public void should_restart_service_if_service_was_running_and_update_fails()
        {
            WithInstalledService();
            WithServiceRunning(true);

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            VerifyServiceRestart();
            ExceptionVerification.ExpectedFatals(1);
        }

        [Test]
        public void should_restart_process_if_service_was_not_running_and_update_fails()
        {
            WithInstalledService();
            WithServiceRunning(false);

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            //Act
            Mocker.Resolve<UpdateProvider>().Start(TARGET_FOLDER);

            //Assert
            VerifyProcessRestart();
            ExceptionVerification.ExpectedFatals(1);
        }

        private void VerifyServiceRestart()
        {
            Mocker.GetMock<ServiceProvider>()
                .Verify(c => c.Start(ServiceProvider.NZBDRONE_SERVICE_NAME), Times.Once());

            Mocker.GetMock<ProcessProvider>()
                .Verify(c => c.Start(It.IsAny<string>()), Times.Never());
        }

        private void VerifyProcessRestart()
        {
            Mocker.GetMock<ServiceProvider>()
                .Verify(c => c.Start(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<ProcessProvider>()
                .Verify(c => c.Start(TARGET_FOLDER + "NzbDrone.exe"), Times.Once());
        }


    }
}
