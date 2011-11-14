// ReSharper disable InconsistentNaming
using System;
using System.IO;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderVerifyFixture : TestBase
    {

        AutoMoqer mocker = new AutoMoqer();

        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();

            mocker.GetMock<EnviromentProvider>()
                .Setup(c => c.StartUpPath).Returns(@"C:\Temp\NzbDrone_update\");

            mocker.GetMock<EnviromentProvider>()
                .Setup(c => c.SystemTemp).Returns(@"C:\Temp\");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void update_should_throw_target_folder_is_blank(string target)
        {
            Assert.Throws<ArgumentException>(() => mocker.Resolve<UpdateProvider>().Start(target))
            .Message.Should().StartWith("Target folder can not be null or empty");
        }

        [Test]
        public void update_should_throw_if_target_folder_doesnt_exist()
        {
            string targetFolder = "c:\\NzbDrone\\";

            Assert.Throws<DirectoryNotFoundException>(() => mocker.Resolve<UpdateProvider>().Start(targetFolder))
            .Message.Should().StartWith("Target folder doesn't exist");
        }

        [Test]
        public void update_should_throw_if_update_folder_doesnt_exist()
        {
            const string sandboxFolder = @"C:\Temp\NzbDrone_update\nzbdrone";
            const string targetFolder = "c:\\NzbDrone\\";

            mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(targetFolder))
                .Returns(true);

            mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(sandboxFolder))
               .Returns(false);

            Assert.Throws<DirectoryNotFoundException>(() => mocker.Resolve<UpdateProvider>().Start(targetFolder))
                .Message.Should().StartWith("Update folder doesn't exist");
        }
    }
}
