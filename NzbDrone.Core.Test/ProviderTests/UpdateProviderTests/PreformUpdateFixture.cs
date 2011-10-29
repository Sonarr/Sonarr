using System;
using System.IO;
using AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using DiskProvider = NzbDrone.Core.Providers.Core.DiskProvider;

namespace NzbDrone.Core.Test.ProviderTests.UpdateProviderTests
{
    [TestFixture]
    internal class PreformUpdateFixture : TestBase
    {
        private AutoMoqer _mocker = null;

        [SetUp]
        public void setup()
        {
            _mocker = new AutoMoqer(MockBehavior.Strict);
            _mocker.GetMock<EnviromentProvider>()
                .SetupGet(c => c.TempPath).Returns(TempFolder);
        }


        [Test]
        public void Should_call_download_and_extract_using_correct_arguments()
        {
            //Act
            var updatePackage = new UpdatePackage
                                    {
                                        FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
                                        Url = "http://update.nzbdrone.com/kayone/NzbDrone.kay.one.0.6.0.2031.zip",
                                        Version = new Version("0.6.0.2031")
                                    };

            _mocker.GetMock<HttpProvider>().Setup(
                c => c.DownloadFile(updatePackage.Url, Path.Combine(TempFolder, UpdateProvider.SandboxFolderName ,updatePackage.FileName)));

            _mocker.GetMock<DiskProvider>().Setup(
               c => c.ExtractArchive(Path.Combine(TempFolder, UpdateProvider.SandboxFolderName, updatePackage.FileName),
                   Path.Combine(TempFolder, UpdateProvider.SandboxFolderName)));

            _mocker.Resolve<UpdateProvider>().PreformUpdate(updatePackage);
        }

        [Test]
        public void Should_download_and_extract_to_temp_folder()
        {

            var updateSubFolder = new DirectoryInfo(Path.Combine(TempFolder, UpdateProvider.SandboxFolderName));

            var updatePackage = new UpdatePackage
                                    {
                                        FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
                                        Url = "http://update.nzbdrone.com/_test/NzbDrone.zip",
                                        Version = new Version("0.6.0.2031")
                                    };


            //Act
            updateSubFolder.Exists.Should().BeFalse();

            _mocker.Resolve<HttpProvider>();
            _mocker.Resolve<DiskProvider>();
            _mocker.Resolve<UpdateProvider>().PreformUpdate(updatePackage);
            updateSubFolder.Refresh();
            //Assert

            updateSubFolder.Exists.Should().BeTrue();
            updateSubFolder.GetDirectories("nzbdrone").Should().HaveCount(1);
            updateSubFolder.GetDirectories().Should().HaveCount(1);
            updateSubFolder.GetFiles().Should().HaveCount(1);
        }

    }
}
