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
    internal class PreformUpdateFixture : CoreTest
    {


        private const string SANDBOX_FOLDER = @"C:\Temp\nzbdrone_update\";

        [SetUp]
        public void setup()
        {
            WithStrictMocker();

           
        }


        [Test]
        public void Should_call_download_and_extract_using_correct_arguments()
        {
            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.SystemTemp).Returns(@"C:\Temp\");

            var updatePackage = new UpdatePackage
                                    {
                                        FileName = "NzbDrone.kay.one.0.6.0.2031.zip",
                                        Url = "http://update.nzbdrone.com/kayone/NzbDrone.kay.one.0.6.0.2031.zip",
                                        Version = new Version("0.6.0.2031")
                                    };

            var updateArchive = Path.Combine(SANDBOX_FOLDER, updatePackage.FileName);

            Mocker.GetMock<HttpProvider>().Setup(
                c => c.DownloadFile(updatePackage.Url, updateArchive));

            Mocker.GetMock<ArchiveProvider>().Setup(
               c => c.ExtractArchive(updateArchive, SANDBOX_FOLDER));

            //Act
            Mocker.Resolve<UpdateProvider>().StartUpgrade(updatePackage);
        }

        [Test]
        public void Should_download_and_extract_to_temp_folder()
        {

            Mocker.GetMock<EnviromentProvider>().SetupGet(c => c.SystemTemp).Returns(TempFolder);

            var updateSubFolder = new DirectoryInfo(Mocker.GetMock<EnviromentProvider>().Object.GetUpdateSandboxFolder());

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
