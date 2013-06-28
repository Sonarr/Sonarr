using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Api.Client
{
    public class ClientSettings : IHandle<ApplicationStartedEvent>
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;

        private static readonly Regex VersionRegex = new Regex(@"(?<=Version:\s')(.*)(?=')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BuildDateRegex = new Regex(@"(?<=BuildDate:\s)('.*')", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ClientSettings(IAppDirectoryInfo appDirectoryInfo)
        {
            _appDirectoryInfo = appDirectoryInfo;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            //TODO: Update the APIKey (when we have it)

            var appFile = Path.Combine(_appDirectoryInfo.StartUpPath, "UI", "app.js");
            var contents = File.ReadAllText(appFile);
            var version = BuildInfo.Version;
            var date = BuildInfo.BuildDateTime;

            contents = VersionRegex.Replace(contents, version.ToString());
            contents = BuildDateRegex.Replace(contents, date.ToUniversalTime().ToJson());

            File.WriteAllText(appFile, contents);
        }
    }
}
