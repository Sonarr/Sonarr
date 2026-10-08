using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRestorationServiceFixture : CoreTest<DatabaseRestorationService>
    {
        private string _databasePath;
        private string _databaseRestorePath;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IAppFolderInfo>().SetupGet(c => c.AppDataFolder).Returns(@"C:\ProgramData\Sonarr".AsOsAgnostic());

            _databasePath = Mocker.GetMock<IAppFolderInfo>().Object.GetDatabase();
            _databaseRestorePath = Mocker.GetMock<IAppFolderInfo>().Object.GetDatabaseRestore();
        }

        private void GivenFileExists(string path)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_do_nothing_if_there_is_no_database_to_restore()
        {
            GivenFileExists(_databasePath);

            Subject.Restore();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_keep_current_database_as_pre_restore()
        {
            GivenFileExists(_databaseRestorePath);
            GivenFileExists(_databasePath);
            GivenFileExists(_databasePath + "-wal");

            Subject.Restore();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(_databasePath, _databasePath + ".pre-restore", false), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(_databasePath + "-wal", _databasePath + ".pre-restore-wal", false), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(_databaseRestorePath, _databasePath, false), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(_databasePath), Times.Never());
        }

        [Test]
        public void should_replace_previous_pre_restore_database()
        {
            GivenFileExists(_databaseRestorePath);
            GivenFileExists(_databasePath);
            GivenFileExists(_databasePath + ".pre-restore");
            GivenFileExists(_databasePath + ".pre-restore-wal");

            Subject.Restore();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(_databasePath + ".pre-restore"), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(_databasePath + ".pre-restore-wal"), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(_databasePath, _databasePath + ".pre-restore", false), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(_databaseRestorePath, _databasePath, false), Times.Once());
        }
    }
}
