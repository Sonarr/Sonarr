using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.UpdateProviderTests
{
    class GetUpdateLogFixture : CoreTest
    {
        String UpdateLogFolder;


        [SetUp]
        public void setup()
        {
            WithTempAsAppPath();

            UpdateLogFolder = Mocker.GetMock<EnviromentProvider>().Object.GetUpdateLogFolder();

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetFiles(UpdateLogFolder, SearchOption.TopDirectoryOnly))
                .Returns(new [] 
                {
                    "C:\\nzbdrone\\update\\2011.09.20-19-08.txt", 
                    "C:\\nzbdrone\\update\\2011.10.20-20-08.txt", 
                    "C:\\nzbdrone\\update\\2011.12.20-21-08.txt" 
                });

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(UpdateLogFolder))
                .Returns(true);
        }


        [Test]
        public void get_logs_should_return_empty_list_if_directory_doesnt_exist()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(UpdateLogFolder))
                .Returns(false);

            var logs = Mocker.Resolve<UpdateProvider>().UpdateLogFile();
            logs.Should().BeEmpty();
        }


        [Test]
        public void get_logs_should_return_list_of_files_in_log_folder()
        {
            var logs = Mocker.Resolve<UpdateProvider>().UpdateLogFile();
            logs.Should().HaveCount(3);
        }

    }
}
