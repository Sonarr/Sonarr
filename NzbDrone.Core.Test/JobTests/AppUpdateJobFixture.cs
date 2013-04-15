using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    internal class AppUpdateJobFixture : CoreTest
    {
        private const string SANDBOX_FOLDER = @"C:\Temp\nzbdrone_update\";

        private readonly Guid _clientGuid = Guid.NewGuid();

        private readonly UpdatePackage updatePackage = new UpdatePackage
        {
            FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
            Url = "http://update.nzbdrone.com/_test/NzbDrone.zip",
            Version = new Version("0.6.0.2031")
        };

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");
            Mocker.GetMock<ConfigFileProvider>().SetupGet(c => c.Guid).Returns(_clientGuid);
            Mocker.GetMock<UpdateService>().Setup(c => c.GetAvailableUpdate()).Returns(updatePackage);
        }


        [Test]
        public void should_delete_sandbox_before_update_if_folder_exists()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FolderExists(SANDBOX_FOLDER)).Returns(true);


            StartUpdate();


            Mocker.GetMock<DiskProvider>().Verify(c => c.DeleteFolder(SANDBOX_FOLDER, true));
        }

        [Test]
        public void should_not_delete_sandbox_before_update_if_folder_doesnt_exists()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FolderExists(SANDBOX_FOLDER)).Returns(false);


            StartUpdate();


            Mocker.GetMock<DiskProvider>().Verify(c => c.DeleteFolder(SANDBOX_FOLDER, true), Times.Never());
        }


        [Test]
        public void Should_download_update_package()
        {
            var updateArchive = Path.Combine(SANDBOX_FOLDER, updatePackage.FileName);


            StartUpdate();


            Mocker.GetMock<IHttpProvider>().Verify(
                    c => c.DownloadFile(updatePackage.Url, updateArchive));
        }

        [Test]
        public void Should_extract_update_package()
        {
            var updateArchive = Path.Combine(SANDBOX_FOLDER, updatePackage.FileName);


            StartUpdate();


            Mocker.GetMock<ArchiveProvider>().Verify(
               c => c.ExtractArchive(updateArchive, SANDBOX_FOLDER));
        }

        [Test]
        public void Should_copy_update_client_to_root_of_sandbox()
        {
            var updateClientFolder = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateClientFolder();


            StartUpdate();


            Mocker.GetMock<DiskProvider>().Verify(
               c => c.MoveDirectory(updateClientFolder, SANDBOX_FOLDER));
        }

        [Test]
        public void should_start_update_client()
        {

            var updateClientPath = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateClientExePath();

            Mocker.GetMock<EnvironmentProvider>()
                .SetupGet(c => c.NzbDroneProcessIdFromEnvironment).Returns(12);


            StartUpdate();


            Mocker.GetMock<ProcessProvider>().Verify(
               c => c.Start(It.Is<ProcessStartInfo>(p =>
                       p.FileName == updateClientPath &&
                       p.Arguments == "12 " + _clientGuid.ToString())
                       ));
        }

        [Test]
        public void when_no_updates_are_available_should_return_without_error_or_warnings()
        {
            Mocker.GetMock<UpdateService>().Setup(c => c.GetAvailableUpdate()).Returns((UpdatePackage)null);

            StartUpdate();

            ExceptionVerification.AssertNoUnexcpectedLogs();
        }

        [Test]
        [Category(INTEGRATION_TEST)]
        public void Should_download_and_extract_to_temp_folder()
        {

            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.SystemTemp).Returns(TempFolder);

            var updateSubFolder = new DirectoryInfo(Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateSandboxFolder());



            updateSubFolder.Exists.Should().BeFalse();

            Mocker.SetConstant<IHttpProvider>(new HttpProvider(new EnvironmentProvider()));
            Mocker.Resolve<DiskProvider>();
            Mocker.Resolve<ArchiveProvider>();
            StartUpdate();
            updateSubFolder.Refresh();


            updateSubFolder.Exists.Should().BeTrue();
            updateSubFolder.GetDirectories("nzbdrone").Should().HaveCount(1);
            updateSubFolder.GetDirectories().Should().HaveCount(1);
            updateSubFolder.GetFiles().Should().HaveCount(1);
        }

        private void StartUpdate()
        {
            Mocker.Resolve<AppUpdateJob>().Start(MockNotification, null);
        }
    }
}
