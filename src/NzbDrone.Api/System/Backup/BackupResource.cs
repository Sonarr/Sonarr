using System;
using Sonarr.Http.REST;
using NzbDrone.Core.Backup;

namespace NzbDrone.Api.System.Backup
{
    public class BackupResource : RestResource
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
