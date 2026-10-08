using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.BackupTests
{
    [TestFixture]
    public class BackupServiceFixture : CoreTest<BackupService>
    {
        private string _configPath;
        private string _databaseRestorePath;
        private string _extractPath;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IAppFolderInfo>().SetupGet(c => c.TempFolder).Returns(TempFolder);
            Mocker.GetMock<IAppFolderInfo>().SetupGet(c => c.AppDataFolder).Returns(@"C:\ProgramData\Sonarr".AsOsAgnostic());

            _configPath = Mocker.GetMock<IAppFolderInfo>().Object.GetConfigPath();
            _databaseRestorePath = Mocker.GetMock<IAppFolderInfo>().Object.GetDatabaseRestore();
            _extractPath = Path.Combine(TempFolder, "sonarr_backup_restore");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.OpenReadStream(It.IsAny<string>()))
                  .Returns<string>(path => new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        private string GivenFile(string fileName, string contents)
        {
            Directory.CreateDirectory(TempFolder);

            var path = Path.Combine(TempFolder, fileName);
            File.WriteAllText(path, contents, Encoding.ASCII);

            return path;
        }

        private string GivenSqliteDatabase(string fileName)
        {
            return GivenFile(fileName, "SQLite format 3\0" + new string('\0', 84));
        }

        private string GivenXmlFile(string fileName)
        {
            return GivenFile(fileName, "<Config><Port>8989</Port></Config>");
        }

        private void GivenZipContents(params string[] files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_extractPath, false))
                  .Returns(files);
        }

        [Test]
        public void should_restore_uploaded_xml_as_config()
        {
            var path = GivenXmlFile("sonarr_backup_restore.xml");

            Subject.Restore(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(path, _configPath, true), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(It.IsAny<string>(), _databaseRestorePath, It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_restore_uploaded_sqlite_database()
        {
            var path = GivenSqliteDatabase("sonarr_backup_restore.db");

            Subject.Restore(path);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(path, _databaseRestorePath, true), Times.Once());
        }

        [Test]
        public void should_not_restore_uploaded_database_that_is_not_sqlite()
        {
            var path = GivenXmlFile("sonarr_backup_restore.db");

            Assert.Throws<RestoreBackupFailedException>(() => Subject.Restore(path));

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_not_restore_empty_database()
        {
            var path = GivenFile("sonarr_backup_restore.db", "");

            Assert.Throws<RestoreBackupFailedException>(() => Subject.Restore(path));

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_restore_config_and_database_from_zip()
        {
            var config = GivenXmlFile("config.xml");
            var database = GivenSqliteDatabase("sonarr.db");

            GivenZipContents(config, database);

            Subject.Restore("sonarr_backup.zip");

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(config, _configPath, true), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(database, _databaseRestorePath, true), Times.Once());
        }

        [Test]
        public void should_not_restore_database_from_zip_that_is_not_sqlite()
        {
            var database = GivenXmlFile("sonarr.db");

            GivenZipContents(database);

            Assert.Throws<RestoreBackupFailedException>(() => Subject.Restore("sonarr_backup.zip"));

            Mocker.GetMock<IDiskProvider>().Verify(v => v.MoveFile(It.IsAny<string>(), _databaseRestorePath, It.IsAny<bool>()), Times.Never());
        }
    }
}
