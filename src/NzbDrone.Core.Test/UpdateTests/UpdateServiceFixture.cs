using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;
using NzbDrone.Core.Update.Commands;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.UpdateTests
{
    [TestFixture]
    public class UpdateServiceFixture : CoreTest<InstallUpdateService>
    {
        private string _sandboxFolder;

        private UpdatePackage _updatePackage;

        [SetUp]
        public void Setup()
        {
            if (OsInfo.IsLinux)
            {
                _updatePackage = new UpdatePackage
                {
                    FileName = "NzbDrone.develop.2.0.0.0.tar.gz",
                    Url = "http://update.nzbdrone.com/v2/develop/mono/NzbDrone.develop.tar.gz",
                    Version = new Version("2.0.0.0")
                };
            }

            else
            {
                _updatePackage = new UpdatePackage
                {
                    FileName = "NzbDrone.develop.2.0.0.0.zip",
                    Url = "http://update.nzbdrone.com/v2/develop/windows/NzbDrone.develop.zip",
                    Version = new Version("2.0.0.0")
                };
            }

            Mocker.GetMock<IAppFolderInfo>().SetupGet(c => c.TempFolder).Returns(TempFolder);
            Mocker.GetMock<ICheckUpdateService>().Setup(c => c.AvailableUpdate()).Returns(_updatePackage);

            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetCurrentProcess()).Returns(new ProcessInfo { Id = 12 });

            _sandboxFolder = Mocker.GetMock<IAppFolderInfo>().Object.GetUpdateSandboxFolder();
        }


        [Test]
        public void should_delete_sandbox_before_update_if_folder_exists()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(_sandboxFolder)).Returns(true);

            Subject.Execute(new ApplicationUpdateCommand());

            Mocker.GetMock<IDiskProvider>().Verify(c => c.DeleteFolder(_sandboxFolder, true));
        }

        [Test]
        public void should_not_delete_sandbox_before_update_if_folder_doesnt_exists()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(_sandboxFolder)).Returns(false);

            Subject.Execute(new ApplicationUpdateCommand());


            Mocker.GetMock<IDiskProvider>().Verify(c => c.DeleteFolder(_sandboxFolder, true), Times.Never());
        }


        [Test]
        public void Should_download_update_package()
        {
            var updateArchive = Path.Combine(_sandboxFolder, _updatePackage.FileName);

            Subject.Execute(new ApplicationUpdateCommand());


            Mocker.GetMock<IHttpProvider>().Verify(c => c.DownloadFile(_updatePackage.Url, updateArchive));
        }

        [Test]
        public void Should_extract_update_package()
        {
            var updateArchive = Path.Combine(_sandboxFolder, _updatePackage.FileName);

            Subject.Execute(new ApplicationUpdateCommand());


            Mocker.GetMock<IArchiveService>().Verify(c => c.Extract(updateArchive, _sandboxFolder));
        }

        [Test]
        public void Should_copy_update_client_to_root_of_sandbox()
        {
            var updateClientFolder = Mocker.GetMock<IAppFolderInfo>().Object.GetUpdateClientFolder();

            Subject.Execute(new ApplicationUpdateCommand());



            Mocker.GetMock<IDiskProvider>().Verify(c => c.MoveFolder(updateClientFolder, _sandboxFolder));
        }

        [Test]
        public void should_start_update_client()
        {
            Subject.Execute(new ApplicationUpdateCommand());

            Mocker.GetMock<IProcessProvider>()
                .Verify(c => c.Start(It.IsAny<string>(), "12", null, null), Times.Once());
        }

        [Test]
        public void when_no_updates_are_available_should_return_without_error_or_warnings()
        {
            Mocker.GetMock<ICheckUpdateService>().Setup(c => c.AvailableUpdate()).Returns<UpdatePackage>(null);

            Subject.Execute(new ApplicationUpdateCommand());


            ExceptionVerification.AssertNoUnexcpectedLogs();
        }

        [Test]
        [IntegrationTest]
        public void Should_download_and_extract_to_temp_folder()
        {
            UseRealHttp();

            var updateSubFolder = new DirectoryInfo(Mocker.GetMock<IAppFolderInfo>().Object.GetUpdateSandboxFolder());

            updateSubFolder.Exists.Should().BeFalse();

            Mocker.SetConstant<IArchiveService>(Mocker.Resolve<ArchiveService>());

            Subject.Execute(new ApplicationUpdateCommand());

            updateSubFolder.Refresh();

            updateSubFolder.Exists.Should().BeTrue();
            updateSubFolder.GetDirectories("nzbdrone").Should().HaveCount(1);
            updateSubFolder.GetDirectories().Should().HaveCount(1);
            updateSubFolder.GetFiles().Should().NotBeEmpty();
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.IgnoreErrors();
        }
    }
}
