using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Backup;
using NzbDrone.Test.Common;
using Sonarr.Http.Frontend.Mappers;

namespace Sonarr.Http.Test.Frontend.Mappers
{
    [TestFixture]
    public class BackupFileMapperFixture : TestBase<BackupFileMapper>
    {
        private string _backupFolder;

        [SetUp]
        public void Setup()
        {
            _backupFolder = Path.Combine(TempFolder, "Backups");
        }

        [Test]
        public void should_resolve_backup_path_without_trailing_separator()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(c => c.GetBackupFolder())
                .Returns(_backupFolder);

            var expected = Path.Combine(_backupFolder, "manual", "nzbdrone_backup_2024.zip");

            Subject.Map("/backup/manual/nzbdrone_backup_2024.zip").Should().Be(expected);
        }

        [Test]
        public void should_resolve_backup_path_with_trailing_separator()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(c => c.GetBackupFolder())
                .Returns(_backupFolder + Path.DirectorySeparatorChar);

            var expected = Path.Combine(_backupFolder, "manual", "nzbdrone_backup_2024.zip");

            Subject.Map("/backup/manual/nzbdrone_backup_2024.zip").Should().Be(expected);
        }

        [Test]
        public void should_return_null_when_resource_url_traverses_outside_backup_folder()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(c => c.GetBackupFolder())
                .Returns(_backupFolder);

            Subject.Map("/backup/../nzbdrone.db").Should().BeNull();
        }
    }
}
