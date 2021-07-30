using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Mono.EnvironmentInfo.VersionAdapters
{
    public class ReleaseFileVersionAdapter : IOsVersionAdapter
    {
        private readonly IDiskProvider _diskProvider;

        public ReleaseFileVersionAdapter(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public OsVersionModel Read()
        {
            if (!_diskProvider.FolderExists("/etc/"))
            {
                return null;
            }

            var releaseFiles = _diskProvider.GetFiles("/etc/", SearchOption.TopDirectoryOnly).Where(c => c.EndsWith("release")).ToList();

            var name = "Linux";
            var fullName = "";
            var version = "";

            bool success = false;

            foreach (var releaseFile in releaseFiles)
            {
                var fileContent = _diskProvider.ReadAllText(releaseFile);
                var lines = Regex.Split(fileContent, "\r\n|\r|\n");

                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length >= 2)
                    {
                        var key = parts[0];
                        var value = parts[1];

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            switch (key)
                            {
                                case "ID":
                                    success = true;
                                    name = value;
                                    break;
                                case "PRETTY_NAME":
                                    success = true;
                                    fullName = value;
                                    break;
                                case "VERSION_ID":
                                    success = true;
                                    version = value;
                                    break;
                            }
                        }
                    }
                }
            }

            if (!success)
            {
                return null;
            }

            return new OsVersionModel(name, version, fullName);

        }

        public bool Enabled => OsInfo.IsLinux;
    }
}
