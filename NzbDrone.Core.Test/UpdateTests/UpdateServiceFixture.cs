using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.UpdateTests
{
    [TestFixture]
    public class UpdateServiceFixture : CoreTest<UpdateService>
    {
        private string _sandboxFolder;

        private readonly Guid _clientGuid = Guid.NewGuid();

        private readonly UpdatePackage _updatePackage = new UpdatePackage
        {
            FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
            Url = "http://update.nzbdrone.com/_test/NzbDrone.zip",
            Version = new Version("0.6.0.2031")
        };

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.SystemTemp).Returns(TempFolder);
            Mocker.GetMock<ConfigFileProvider>().SetupGet(c => c.Guid).Returns(_clientGuid);
            Mocker.GetMock<IUpdatePackageProvider>().Setup(c => c.GetLatestUpdate()).Returns(_updatePackage);

            Mocker.GetMock<ProcessProvider>().Setup(c => c.GetCurrentProcess()).Returns(new ProcessInfo { Id = 12 });

            _sandboxFolder = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateSandboxFolder();
        }



        [Test]
        public void should_delete_sandbox_before_update_if_folder_exists()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FolderExists(_sandboxFolder)).Returns(true);

            Subject.InstallAvailableUpdate();

            Mocker.GetMock<DiskProvider>().Verify(c => c.DeleteFolder(_sandboxFolder, true));
        }

        [Test]
        public void should_not_delete_sandbox_before_update_if_folder_doesnt_exists()
        {
            Mocker.GetMock<DiskProvider>().Setup(c => c.FolderExists(_sandboxFolder)).Returns(false);

            Subject.InstallAvailableUpdate();

            Mocker.GetMock<DiskProvider>().Verify(c => c.DeleteFolder(_sandboxFolder, true), Times.Never());
        }


        [Test]
        public void Should_download_update_package()
        {
            var updateArchive = Path.Combine(_sandboxFolder, _updatePackage.FileName);

            Subject.InstallAvailableUpdate();

            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(_updatePackage.Url, updateArchive));
        }

        [Test]
        public void Should_extract_update_package()
        {
            var updateArchive = Path.Combine(_sandboxFolder, _updatePackage.FileName);

            Subject.InstallAvailableUpdate();

            Mocker.GetMock<ArchiveProvider>().Verify(c => c.ExtractArchive(updateArchive, _sandboxFolder));
        }

        [Test]
        public void Should_copy_update_client_to_root_of_sandbox()
        {
            var updateClientFolder = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateClientFolder();

            Subject.InstallAvailableUpdate();


            Mocker.GetMock<DiskProvider>().Verify(c => c.MoveDirectory(updateClientFolder, _sandboxFolder));
        }

        [Test]
        public void should_start_update_client()
        {
            var updateClientPath = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateClientExePath();



            Subject.InstallAvailableUpdate();


            Mocker.GetMock<ProcessProvider>().Verify(
               c => c.Start(It.Is<ProcessStartInfo>(p =>
                       p.FileName == updateClientPath &&
                       p.Arguments == "12 " + _clientGuid.ToString())
                       ));
        }

        [Test]
        public void when_no_updates_are_available_should_return_without_error_or_warnings()
        {
            Mocker.GetMock<IUpdatePackageProvider>().Setup(c => c.GetLatestUpdate()).Returns<UpdatePackage>(null);

            Subject.InstallAvailableUpdate();

            ExceptionVerification.AssertNoUnexcpectedLogs();
        }

        [Test]
        [IntegrationTest]
        public void Should_download_and_extract_to_temp_folder()
        {
            UseRealHttp();

            var updateSubFolder = new DirectoryInfo(Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateSandboxFolder());

            updateSubFolder.Exists.Should().BeFalse();

            Mocker.Resolve<DiskProvider>();
            Mocker.Resolve<ArchiveProvider>();

            Subject.InstallAvailableUpdate();

            updateSubFolder.Refresh();

            updateSubFolder.Exists.Should().BeTrue();
            updateSubFolder.GetDirectories("nzbdrone").Should().HaveCount(1);
            updateSubFolder.GetDirectories().Should().HaveCount(1);
            updateSubFolder.GetFiles().Should().NotBeEmpty();
        }
    }
}
