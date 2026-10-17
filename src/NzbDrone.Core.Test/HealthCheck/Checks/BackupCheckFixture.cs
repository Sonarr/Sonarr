using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class BackupCheckFixture : CoreTest<BackupCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.BackupInterval)
                .Returns(1);
        }

        [Test]
        public void should_be_ok_when_no_scheduled_backup_exists_yet()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(s => s.GetBackups())
                .Returns(new List<Backup>());

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_be_ok_when_latest_scheduled_backup_is_recent()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(s => s.GetBackups())
                .Returns(new List<Backup>
                {
                    new Backup
                    {
                        Name = "sonarr_backup.zip",
                        Type = BackupType.Scheduled,
                        Time = DateTime.UtcNow.AddHours(-12)
                    }
                });

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_error_when_latest_scheduled_backup_is_overdue()
        {
            Mocker.GetMock<IBackupService>()
                .Setup(s => s.GetBackups())
                .Returns(new List<Backup>
                {
                    new Backup
                    {
                        Name = "sonarr_backup.zip",
                        Type = BackupType.Scheduled,
                        Time = DateTime.UtcNow.AddDays(-3)
                    }
                });

            Subject.Check().ShouldBeError();
        }
    }
}
