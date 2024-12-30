﻿using Workarr.Messaging.Commands;

namespace Workarr.Backup
{
    public class BackupCommand : Command
    {
        public BackupType Type
        {
            get
            {
                if (Trigger == CommandTrigger.Scheduled)
                {
                    return BackupType.Scheduled;
                }

                return BackupType.Manual;
            }
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => Type == BackupType.Scheduled;
    }

    public enum BackupType
    {
        Scheduled = 0,
        Manual = 1,
        Update = 2
    }
}
