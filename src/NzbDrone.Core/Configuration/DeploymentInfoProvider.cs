using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Update;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NzbDrone.Core.Configuration
{
    public interface IDeploymentInfoProvider
    {
        Version PackageVersion { get; }
        string PackageBranch { get; }
        UpdateMechanism PackageUpdateMechanism { get; }

        Version ReleaseVersion { get; }
        string ReleaseBranch { get; }

        bool IsExternalUpdateMechanism { get; }
        UpdateMechanism DefaultUpdateMechanism { get; }
        string DefaultBranch { get; }
    }

    public class DeploymentInfoProvider : IDeploymentInfoProvider
    {
        public DeploymentInfoProvider(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider)
        {
            var bin = appFolderInfo.StartUpFolder;
            var packageInfoPath = Path.Combine(bin, "..", "package_info");
            var releaseInfoPath = Path.Combine(bin, "release_info");

            PackageUpdateMechanism = UpdateMechanism.BuiltIn;
            DefaultBranch = "master";

            if (Path.GetFileName(bin) == "bin" && diskProvider.FileExists(packageInfoPath))
            {
                var data = diskProvider.ReadAllText(packageInfoPath);

                PackageVersion = ReadVersion(data, "PackageVersion");
                PackageUpdateMechanism = ReadEnumValue(data, "UpdateMethod", UpdateMechanism.BuiltIn);
                PackageBranch = ReadValue(data, "Branch", null);

                ReleaseVersion = ReadVersion(data, "ReleaseVersion");

                if (PackageBranch.IsNotNullOrWhiteSpace())
                {
                    DefaultBranch = PackageBranch;
                }
            }

            if (diskProvider.FileExists(releaseInfoPath))
            {
                var data = diskProvider.ReadAllText(releaseInfoPath);

                ReleaseVersion = ReadVersion(data, "ReleaseVersion", ReleaseVersion);
                ReleaseBranch = ReadValue(data, "Branch", null);

                if (ReleaseBranch.IsNotNullOrWhiteSpace())
                {
                    DefaultBranch = ReleaseBranch;
                }
            }

            DefaultUpdateMechanism = PackageUpdateMechanism;
        }

        private static string ReadValue(string fileData, string key, string defaultValue)
        {
            var match = Regex.Match(fileData, "^" + key + "=(.*)$", RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return defaultValue;
        }

        private static T ReadEnumValue<T>(string fileData, string key, T defaultValue)
            where T : struct
        {
            var value = ReadValue(fileData, key, null);
            if (value != null && Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        private static Version ReadVersion(string fileData, string key, Version defaultValue = null)
        {
            var value = ReadValue(fileData, key, null);
            if (value != null && Version.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        public Version PackageVersion { get; private set; }
        public string PackageBranch { get; private set; }
        public UpdateMechanism PackageUpdateMechanism { get; private set; }

        public Version ReleaseVersion { get; set; }
        public string ReleaseBranch { get; set; }


        public bool IsExternalUpdateMechanism => PackageUpdateMechanism >= UpdateMechanism.External;
        public UpdateMechanism DefaultUpdateMechanism { get; private set; }
        public string DefaultBranch { get; private set; }
    }
}
