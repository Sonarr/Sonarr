/*
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update.Test
{
    [TestFixture]
   public class UpdateProviderStartFixture : TestBase
    {
        private const string UPDATE_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone\";
        private const string BACKUP_FOLDER = @"C:\Temp\nzbdrone_update\nzbdrone_backup\";
        private const string TARGET_FOLDER = @"C:\NzbDrone\";

        Mock<IIAppDirectoryInfo> _IAppDirectoryInfo;


        [SetUp]
        public void Setup()
        {

            _IAppDirectoryInfo = Mocker.GetMock<IIAppDirectoryInfo>();

            _IAppDirectoryInfo.SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists(UPDATE_FOLDER))
               .Returns(true);

            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists(TARGET_FOLDER))
               .Returns(true);
        }

        private void WithInstalledService()
        {
            Mocker.GetMock<IServiceProvider>()
              .Setup(c => c.ServiceExist(ServiceProvider.SERVICE_NAME))
              .Returns(true);
        }

        private void WithServiceRunning(bool state)
        {
            Mocker.GetMock<IServiceProvider>()
                .Setup(c => c.IsServiceRunning(ServiceProvider.SERVICE_NAME)).Returns(state);
        }

        [Test]
        public void should_stop_nzbdrone_service_if_installed_and_running()
        {
            WithInstalledService();
            WithServiceRunning(true);

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            Mocker.GetMock<IServiceProvider>().Verify(c => c.Stop(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void should_not_stop_nzbdrone_service_if_installed_but_not_running()
        {
            WithInstalledService();
            WithServiceRunning(false);

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            Mocker.GetMock<IServiceProvider>().Verify(c => c.Stop(ServiceProvider.SERVICE_NAME), Times.Never());
        }

        [Test]
        public void should_not_stop_nzbdrone_service_if_service_isnt_installed()
        {
            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            Mocker.GetMock<IServiceProvider>().Verify(c => c.Stop(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_kill_nzbdrone_process_if_running()
        {
            var proccesses = Builder<ProcessInfo>.CreateListOfSize(2).Build().ToList();

            Mocker.GetMock<IProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProcessName))
                .Returns(proccesses);

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);


            Mocker.GetMock<IProcessProvider>().Verify(c => c.KillAll(ProcessProvider.NzbDroneProcessName), Times.Once());
        }

        [Test]
        public void should_not_kill_nzbdrone_process_not_running()
        {
            Mocker.GetMock<IProcessProvider>()
                .Setup(c => c.GetProcessByName(ProcessProvider.NzbDroneProcessName))
                .Returns(new List<ProcessInfo>());

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            Mocker.GetMock<IProcessProvider>().Verify(c => c.Kill(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_create_backup_of_current_installation()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.CopyDirectory(TARGET_FOLDER, BACKUP_FOLDER));

            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);
        }

        [Test]
        public void should_copy_update_package_to_target()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER));

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.DeleteFolder(UPDATE_FOLDER, true));

            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);
        }

        [Test]
        public void should_restore_if_update_fails()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.CopyDirectory(BACKUP_FOLDER, TARGET_FOLDER), Times.Once());
            ExceptionVerification.ExpectedFatals(1);
        }

        [Test]
        public void should_restart_service_if_service_was_running()
        {
            WithInstalledService();
            WithServiceRunning(true);

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            VerifyServiceRestart();
        }

        [Test]
        public void should_restart_process_if_service_was_not_running()
        {
            WithInstalledService();
            WithServiceRunning(false);

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            VerifyProcessRestart();
        }

        [Test]
        public void should_restart_service_if_service_was_running_and_update_fails()
        {
            WithInstalledService();
            WithServiceRunning(true);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            VerifyServiceRestart();
            ExceptionVerification.ExpectedFatals(1);
        }

        [Test]
        public void should_restart_process_if_service_was_not_running_and_update_fails()
        {
            WithInstalledService();
            WithServiceRunning(false);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.CopyDirectory(UPDATE_FOLDER, TARGET_FOLDER))
                .Throws(new IOException());

            
            Mocker.Resolve<InstallUpdateService>().Start(TARGET_FOLDER);

            
            VerifyProcessRestart();
            ExceptionVerification.ExpectedFatals(1);
        }

        private void VerifyServiceRestart()
        {
            Mocker.GetMock<IServiceProvider>()
                .Verify(c => c.Start(ServiceProvider.SERVICE_NAME), Times.Once());

            Mocker.GetMock<IProcessProvider>()
                .Verify(c => c.Start(It.IsAny<string>()), Times.Never());
        }

        private void VerifyProcessRestart()
        {
            Mocker.GetMock<IServiceProvider>()
                .Verify(c => c.Start(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IProcessProvider>()
                .Verify(c => c.Start(TARGET_FOLDER + "Sonarr.exe"), Times.Once());
        }


    }
}
*/
