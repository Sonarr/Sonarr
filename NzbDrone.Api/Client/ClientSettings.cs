using System.IO;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Api.Client
{
    public class ClientSettings : IHandle<ApplicationStartedEvent>
    {
        private readonly IAppFolderInfo _appFolderInfo;

        private static readonly Regex VersionRegex = new Regex(@"(?<=Version:\s')(.*)(?=')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BuildDateRegex = new Regex(@"(?<=BuildDate:\s)('.*')", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ClientSettings(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            //TODO: Update the APIKey (when we have it)

            var appFile = Path.Combine(_appFolderInfo.StartUpFolder, "UI", "app.js");
            var contents = File.ReadAllText(appFile);
            var version = BuildInfo.Version;
            var date = BuildInfo.BuildDateTime;

            contents = VersionRegex.Replace(contents, version.ToString());
            contents = BuildDateRegex.Replace(contents, date.ToUniversalTime().ToJson());

            File.WriteAllText(appFile, contents);
        }
    }
}
