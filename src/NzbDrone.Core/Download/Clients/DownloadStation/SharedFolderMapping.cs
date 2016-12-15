using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class SharedFolderMapping
    {
        public OsPath PhysicalPath { get; private set; }
        public OsPath SharedFolder { get; private set; }

        public SharedFolderMapping(string sharedFolder, string physicalPath)
        {
            SharedFolder = new OsPath(sharedFolder);
            PhysicalPath = new OsPath(physicalPath);
        }

        public override string ToString()
        {
            return $"{SharedFolder} -> {PhysicalPath}";
        }
    }
}
