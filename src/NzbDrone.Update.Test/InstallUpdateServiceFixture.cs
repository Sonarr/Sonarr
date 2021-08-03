using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Test.Common;
using NzbDrone.Update.UpdateEngine;

namespace NzbDrone.Update.Test
{
    [TestFixture]
    public class InstallUpdateServiceFixture : TestBase<InstallUpdateService>
    {
        private const int _processId = 12;
        private string _targetFolder = @"C:\NzbDrone\".AsOsAgnostic();

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(c => c.TempFolder).Returns(@"C:\Temp\");
        }

        private void GivenTargetFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(_targetFolder))
                  .Returns(true);
        }

        private void GivenProcessExists()
        {
            Mocker.GetMock<IProcessProvider>()
                  .Setup(c => c.Exists(_processId))
                  .Returns(true);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void update_should_throw_target_folder_is_blank(string target)
        {
            Assert.Throws<ArgumentException>(() => Subject.Start(target, _processId))
            .Message.Should().StartWith("Target folder can not be null or empty");
        }

        [Test]
        public void update_should_throw_if_target_folder_doesnt_exist()
        {
            Assert.Throws<DirectoryNotFoundException>(() => Subject.Start(_targetFolder, _processId))
            .Message.Should().StartWith("Target folder doesn't exist");
        }

        [Test]
        public void update_should_throw_if_update_folder_doesnt_exist()
        {
            const string sandboxFolder = @"C:\Temp\NzbDrone_update\nzbdrone";

            GivenTargetFolderExists();
            GivenProcessExists();

            Mocker.GetMock<IDiskProvider>()
               .Setup(c => c.FolderExists(sandboxFolder))
               .Returns(false);

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Start(_targetFolder, _processId))
                .Message.Should().StartWith("Update folder doesn't exist");
        }

        [Test]
        public void update_should_throw_if_process_is_zero()
        {
            GivenTargetFolderExists();

            Assert.Throws<ArgumentException>(() => Subject.Start(_targetFolder, 0))
            .Message.Should().StartWith("Invalid process ID");
        }

        [Test]
        public void update_should_throw_if_process_id_doesnt_exist()
        {
            GivenTargetFolderExists();

            Assert.Throws<ArgumentException>(() => Subject.Start(_targetFolder, _processId))
            .Message.Should().StartWith("Process with ID doesn't exist");
        }
    }
}
