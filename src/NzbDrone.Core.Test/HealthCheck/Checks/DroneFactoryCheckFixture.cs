using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DroneFactoryCheckFixture : CoreTest<DroneFactoryCheck>
    {
        private const string DRONE_FACTORY_FOLDER = @"C:\Test\Unsorted";

        private void GivenDroneFactoryFolder(bool exists = false)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadedEpisodesFolder)
                  .Returns(DRONE_FACTORY_FOLDER);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(DRONE_FACTORY_FOLDER))
                  .Returns(exists);
        }

        [Test]
        public void should_return_warning_when_drone_factory_folder_is_not_configured()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadedEpisodesFolder)
                  .Returns("");

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_when_drone_factory_folder_does_not_exist()
        {
            GivenDroneFactoryFolder();

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_when_unable_to_write_to_drone_factory_folder()
        {
            GivenDroneFactoryFolder(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.WriteAllText(It.IsAny<String>(), It.IsAny<String>()))
                  .Throws<Exception>();

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_null_when_no_issues_found()
        {
            GivenDroneFactoryFolder(true);

            Subject.Check().Should().BeNull();
        }
    }
}
