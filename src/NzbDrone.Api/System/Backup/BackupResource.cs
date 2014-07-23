using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Backup;

namespace NzbDrone.Api.System.Backup
{
    public class BackupResource : RestResource
    {
        public String Name { get; set; }
        public String Path { get; set; }
        public BackupType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
