using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Test.Common;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    public class InstallUpdateServiceFixture : TestBase<InstallUpdateService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IEnvironmentProvider>()
                .Setup(c => c.SystemTemp).Returns(@"C:\Temp\");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void update_should_throw_target_folder_is_blank(string target)
        {
            Assert.Throws<ArgumentException>(() => Subject.Start(target))
            .Message.Should().StartWith("Target folder can not be null or empty");
        }

        [Test]
        public void update_should_throw_if_target_folder_doesnt_exist()
        {
            string targetFolder = "c:\\NzbDrone\\";

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Start(targetFolder))
            .Message.Should().StartWith("Target folder doesn't exist");
        }

        [Test]
        public void update_should_throw_if_update_folder_doesnt_exist()
        {
            const string sandboxFolder = @"C:\Temp\NzbDrone_update\nzbdrone";
            const string targetFolder = "c:\\NzbDrone\\";

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(targetFolder))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists(sandboxFolder))
               .Returns(false);

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Start(targetFolder))
                .Message.Should().StartWith("Update folder doesn't exist");
        }
    }
}
