using System;
using System.Linq;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class BackupCheck : IProvideHealthCheck
    {
        private readonly IBackupService _backupService;
        private readonly IConfigService _configService;

        public BackupCheck(IBackupService backupService, IConfigService configService)
        {
            _backupService = backupService;
            _configService = configService;
        }

        public bool CheckOnStartup => true;

        public bool CheckOnSchedule => true;

        public HealthCheck Check()
        {
            var latest = _backupService.GetBackups()
                .Where(b => b.Type == BackupType.Scheduled)
                .OrderByDescending(b => b.Time)
                .FirstOrDefault();

            // Do not alert a new installation before its first scheduled backup.
            if (latest == null)
            {
                return new HealthCheck(GetType());
            }

            var lastBackupUtc = latest.Time.Kind == DateTimeKind.Utc
                ? latest.Time
                : latest.Time.ToUniversalTime();

            // Allow one extra day beyond the configured interval to avoid false
            // positives from delayed scheduling, downtime, or a recent restart.
            var overdueAfter = TimeSpan.FromDays(_configService.BackupInterval + 1);

            if (DateTime.UtcNow - lastBackupUtc <= overdueAfter)
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(
                GetType(),
                HealthCheckResult.Error,
                HealthCheckReason.BackupStale,
                $"Automatic backups appear to be failing. The last scheduled backup completed at {lastBackupUtc:yyyy-MM-dd HH:mm:ss} UTC, more than {_configService.BackupInterval + 1} days ago.");
        }
    }
}
