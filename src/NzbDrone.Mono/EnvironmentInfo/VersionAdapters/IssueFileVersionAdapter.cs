using System.IO;
using System.Linq;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Mono.EnvironmentInfo.VersionAdapters
{
    public class IssueFileVersionAdapter : IOsVersionAdapter
    {
        private readonly IDiskProvider _diskProvider;

        public IssueFileVersionAdapter(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public OsVersionModel Read()
        {
            if (!_diskProvider.FolderExists("/etc/"))
            {
                return null;
            }

            var issueFile = _diskProvider.GetFiles("/etc/", SearchOption.TopDirectoryOnly).SingleOrDefault(c => c.EndsWith("/issue"));

            if (issueFile == null)
            {
                return null;
            }

            var fileContent = _diskProvider.ReadAllText(issueFile);


            // Ubuntu 14.04.5 LTS \n \l
            // Ubuntu 16.04.1 LTS \n \l

            // Fedora/Centos
            // Kernel \r on an \m (\l)

            // Arch Linux \r (\l)
            // Debian GNU/Linux 8 \n \l
            if (fileContent.Contains("Arch Linux"))
            {
                return new OsVersionModel("Arch", "1.0", "Arch Linux");
            }

            return null;
        }

        public bool Enabled => OsInfo.IsLinux;
    }
}
