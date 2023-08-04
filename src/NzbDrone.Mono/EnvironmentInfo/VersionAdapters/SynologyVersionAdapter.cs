using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Mono.EnvironmentInfo.VersionAdapters
{
    public class SynologyVersionAdapter : IOsVersionAdapter
    {
        private const string NAME = "DSM";
        private const string FULL_NAME = "Synology DSM";
        private readonly IDiskProvider _diskProvider;

        public SynologyVersionAdapter(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public OsVersionModel Read()
        {
            if (!_diskProvider.FolderExists("/etc.defaults/"))
            {
                return null;
            }

            var versionFile = _diskProvider.GetFiles("/etc.defaults/", false).SingleOrDefault(c => c.EndsWith("VERSION"));

            if (versionFile == null)
            {
                return null;
            }

            var version = "";
            var major = "";
            var minor = "0";

            var fileContent = _diskProvider.ReadAllText(versionFile);
            var lines = Regex.Split(fileContent, "\r\n|\r|\n");

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length >= 2)
                {
                    var key = parts[0];
                    var value = parts[1].Trim('"');

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        switch (key)
                        {
                            case "productversion":
                                version = value;
                                break;
                            case "majorversion":
                                major = value;
                                break;
                            case "minorversion":
                                minor = value;
                                break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(version) && !string.IsNullOrWhiteSpace(major))
            {
                version = $"{major}.{minor}";
            }

            return new OsVersionModel(NAME, version, $"{FULL_NAME} {version}");
        }

        public bool Enabled => OsInfo.IsLinux;
    }
}
