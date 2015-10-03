using System;

namespace NzbDrone.Core.Backup
{
    public class Backup
    {
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
