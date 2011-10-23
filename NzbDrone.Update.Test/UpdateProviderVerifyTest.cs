// ReSharper disable InconsistentNaming
using System;
using System.IO;
using AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Update.Providers;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    class UpdateProviderVerifyTest
    {

        AutoMoqer mocker = new AutoMoqer();

        [SetUp]
        public void Setup()
        {
            mocker = new AutoMoqer();

            mocker.GetMock<EnviromentProvider>()
                .Setup(c => c.StartUpPath).Returns(@"C:\Temp\NzbDrone_update\");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void verify_should_throw_target_folder_is_blank(string target)
        {
            Assert.Throws<ArgumentException>(() => mocker.Resolve<UpdateProvider>().Verify(target))
            .Message.Should().StartWith("Target folder can not be null or empty");
        }

        [Test]
        public void verify_should_throw_if_target_folder_doesnt_exist()
        {
            string targetFolder = "c:\\NzbDrone\\";

            Assert.Throws<DirectoryNotFoundException>(() => mocker.Resolve<UpdateProvider>().Verify(targetFolder))
            .Message.Should().StartWith("Target folder doesn't exist");
        }

        [Test]
        public void verify_should_throw_if_update_folder_doesnt_exist()
        {
            const string sandboxFolder = @"C:\Temp\NzbDrone_update\nzbdrone_update";
            const string targetFolder = "c:\\NzbDrone\\";

            mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(targetFolder))
                .Returns(true);

            mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(sandboxFolder))
               .Returns(false);

            Assert.Throws<DirectoryNotFoundException>(() => mocker.Resolve<UpdateProvider>().Verify(targetFolder))
                .Message.Should().StartWith("Update folder doesn't exist");
        }

        [Test]
        public void verify_should_pass_if_update_folder_and_target_folder_both_exist()
        {
            const string targetFolder = "c:\\NzbDrone\\";

            mocker.GetMock<DiskProvider>()
               .Setup(c => c.FolderExists(It.IsAny<string>()))
               .Returns(true);

            mocker.Resolve<UpdateProvider>().Verify(targetFolder);

            mocker.VerifyAllMocks();
        }
    }
}
