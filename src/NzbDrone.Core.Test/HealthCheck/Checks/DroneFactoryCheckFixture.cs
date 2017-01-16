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

        private void GivenDroneFactoryFolder(bool exists = false, bool writable = true)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadedEpisodesFolder)
                  .Returns(DRONE_FACTORY_FOLDER);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(DRONE_FACTORY_FOLDER))
                  .Returns(exists);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderWritable(It.IsAny<string>()))
                  .Returns(exists && writable);
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
            GivenDroneFactoryFolder(true, false);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_when_no_issues_found()
        {
            GivenDroneFactoryFolder(true);

            Subject.Check().ShouldBeOk();
        }
    }
}
