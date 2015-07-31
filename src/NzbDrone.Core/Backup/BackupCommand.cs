using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Backup
{
    public class BackupCommand : Command
    {
        public BackupType Type { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public override bool UpdateScheduledTask
        {
            get
            {
                return Type == BackupType.Scheduled;
            }
        }
    }

    public enum BackupType
    {
        Scheduled = 0,
        Manual = 1,
        Update = 2
    }
}
