using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.UpdateTests
{
    class GetUpdateLogFixture : CoreTest<UpdateService>
    {
        String _updateLogFolder;


        [SetUp]
        public void Setup()
        {
            WithTempAsAppPath();

            _updateLogFolder = Mocker.GetMock<EnvironmentProvider>().Object.GetUpdateLogFolder();

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetFiles(_updateLogFolder, SearchOption.TopDirectoryOnly))
                .Returns(new[] 
                {
                    "C:\\nzbdrone\\update\\2011.09.20-19-08.txt", 
                    "C:\\nzbdrone\\update\\2011.10.20-20-08.txt", 
                    "C:\\nzbdrone\\update\\2011.12.20-21-08.txt" 
                });

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(_updateLogFolder))
                .Returns(true);
        }


        [Test]
        public void get_logs_should_return_empty_list_if_directory_doesnt_exist()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(_updateLogFolder))
                .Returns(false);

            Subject.GetUpdateLogFiles().Should().BeEmpty();
        }


        [Test]
        public void get_logs_should_return_list_of_files_in_log_folder()
        {
            var logs = Subject.GetUpdateLogFiles();
            logs.Should().HaveCount(3);
        }

    }
}
