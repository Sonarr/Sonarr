using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.UpdateProviderTests
{
    [TestFixture]
    internal class PreformUpdateFixture : TestBase
    {

        private string SandBoxPath;

        [SetUp]
        public void setup()
        {
            WithStrictMocker();
            Mocker.GetMock<PathProvider>()
                .SetupGet(c => c.UpdateSandboxFolder).Returns(Path.Combine(TempFolder, "NzbDrone_update"));

           
            SandBoxPath = Mocker.GetMock<PathProvider>().Object.UpdateSandboxFolder;

            Mocker.GetMock<PathProvider>()
               .SetupGet(c => c.UpdatePackageFolder).Returns(Path.Combine(SandBoxPath, "NzbDrone"));

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

            Mocker.GetMock<HttpProvider>().Setup(
                c => c.DownloadFile(updatePackage.Url, Path.Combine(SandBoxPath, updatePackage.FileName)));

            Mocker.GetMock<ArchiveProvider>().Setup(
               c => c.ExtractArchive(Path.Combine(SandBoxPath, updatePackage.FileName), SandBoxPath));

            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);
        }

        [Test]
        public void Should_download_and_extract_to_temp_folder()
        {

            var updateSubFolder = new DirectoryInfo(SandBoxPath);

            var updatePackage = new UpdatePackage
                                    {
                                        FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
                                        Url = "http://update.nzbdrone.com/_test/NzbDrone.zip",
                                        Version = new Version("0.6.0.2031")
                                    };


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
