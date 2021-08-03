using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Configuration
{
    public interface IDeploymentInfoProvider
    {
        string PackageVersion { get; }
        string PackageAuthor { get; }
        string PackageGlobalMessage { get; }
        string PackageBranch { get; }
        UpdateMechanism PackageUpdateMechanism { get; }
        string PackageUpdateMechanismMessage { get; }

        string ReleaseVersion { get; }
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
            DefaultBranch = "main";

            if (Path.GetFileName(bin) == "bin" && diskProvider.FileExists(packageInfoPath))
            {
                var data = diskProvider.ReadAllText(packageInfoPath);

                PackageVersion = ReadValue(data, "PackageVersion");
                PackageAuthor = ReadValue(data, "PackageAuthor");
                PackageGlobalMessage = ReadValue(data, "PackageGlobalMessage");
                PackageUpdateMechanism = ReadEnumValue(data, "UpdateMethod", UpdateMechanism.BuiltIn);
                PackageUpdateMechanismMessage = ReadValue(data, "UpdateMethodMessage");
                PackageBranch = ReadValue(data, "Branch");

                ReleaseVersion = ReadValue(data, "ReleaseVersion");

                if (PackageBranch.IsNotNullOrWhiteSpace())
                {
                    DefaultBranch = PackageBranch;
                }
            }

            if (diskProvider.FileExists(releaseInfoPath))
            {
                var data = diskProvider.ReadAllText(releaseInfoPath);

                ReleaseVersion = ReadValue(data, "ReleaseVersion", ReleaseVersion);
                ReleaseBranch = ReadValue(data, "Branch");

                if (ReleaseBranch.IsNotNullOrWhiteSpace())
                {
                    DefaultBranch = ReleaseBranch;
                }
            }

            DefaultUpdateMechanism = PackageUpdateMechanism;
        }

        private static string ReadValue(string fileData, string key, string defaultValue = null)
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
            var value = ReadValue(fileData, key);
            if (value != null && Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        public string PackageVersion { get; private set; }
        public string PackageAuthor { get; private set; }
        public string PackageGlobalMessage { get; private set; }
        public string PackageBranch { get; private set; }
        public UpdateMechanism PackageUpdateMechanism { get; private set; }
        public string PackageUpdateMechanismMessage { get; private set; }

        public string ReleaseVersion { get; private set; }
        public string ReleaseBranch { get; set; }

        public bool IsExternalUpdateMechanism => PackageUpdateMechanism >= UpdateMechanism.External;
        public UpdateMechanism DefaultUpdateMechanism { get; private set; }
        public string DefaultBranch { get; private set; }
    }
}
