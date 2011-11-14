using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ninject.Activation.Strategies;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.UpdateProviderTests
{
    [TestFixture]
    internal class PreformUpdateFixture : CoreTest
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
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");
            Mocker.GetMock<ConfigFileProvider>().SetupGet(c => c.Guid).Returns(_clientGuid);
        }



        [Test]
        public void Should_download_update_package()
        {
            var updateArchive = Path.Combine(SANDBOX_FOLDER, updatePackage.FileName);

            //Act
            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);

            //Assert
            Mocker.GetMock<HttpProvider>().Verify(
                    c => c.DownloadFile(updatePackage.Url, updateArchive));
        }

        [Test]
        public void Should_call_download_and_extract_using_correct_arguments()
        {
            var updateArchive = Path.Combine(SANDBOX_FOLDER, updatePackage.FileName);

            //Act
            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);

            //Assert
            Mocker.GetMock<ArchiveProvider>().Verify(
               c => c.ExtractArchive(updateArchive, SANDBOX_FOLDER));
        }

        [Test]
        public void should_start_update_client()
        {
            //Setup
            var updateClientPath = Mocker.GetMock<EnviromentProvider>().Object.GetUpdateClientExePath();

            Mocker.GetMock<EnviromentProvider>()
                .SetupGet(c => c.NzbDroneProcessIdFromEnviroment).Returns(12);

            //Act
            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);

            //Assert
            Mocker.GetMock<ProcessProvider>().Verify(
               c => c.Start(It.Is<ProcessStartInfo>(p =>
                       p.FileName == updateClientPath &&
                       p.Arguments == "/12 /" + _clientGuid.ToString())
                       ));
        }

        [Test]
        [Category(IntegrationTest)]
        public void Should_download_and_extract_to_temp_folder()
        {

            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.SystemTemp).Returns(TempFolder);

            var updateSubFolder = new DirectoryInfo(Mocker.GetMock<EnviromentProvider>().Object.GetUpdateSandboxFolder());


            //Act
            updateSubFolder.Exists.Should().BeFalse();

            Mocker.Resolve<HttpProvider>();
            Mocker.Resolve<DiskProvider>();
            Mocker.Resolve<ArchiveProvider>();
            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);
            updateSubFolder.Refresh();
            //Assert

            updateSubFolder.Exists.Should().BeTrue();
            updateSubFolder.GetDirectories("nzbdrone").Should().HaveCount(1);
            updateSubFolder.GetDirectories().Should().HaveCount(1);
            updateSubFolder.GetFiles().Should().HaveCount(1);
        }
    }
}
