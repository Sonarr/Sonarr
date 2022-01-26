using System;

namespace NzbDrone.Core.Backup
{
    public class Backup
    {
        public string Name { get; set; }
        public BackupType Type { get; set; }
        public long Size { get; set; }
        public DateTime Time { get; set; }
    }
}
