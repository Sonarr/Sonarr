using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;


namespace NzbDrone.Core.Providers
{
    public class UpdateProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly IConfigService _configService;
        private readonly EnvironmentProvider _environmentProvider;

        private readonly DiskProvider _diskProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex parseRegex = new Regex(@"(?:\>)(?<filename>NzbDrone.+?(?<version>\d+\.\d+\.\d+\.\d+).+?)(?:\<\/A\>)", RegexOptions.IgnoreCase);
        public const string DEFAULT_UPDATE_URL = @"http://update.nzbdrone.com/_release/";


        public UpdateProvider(HttpProvider httpProvider, IConfigService configService,
            EnvironmentProvider environmentProvider, DiskProvider diskProvider)
        {
            _httpProvider = httpProvider;
            _configService = configService;
            _environmentProvider = environmentProvider;
            _diskProvider = diskProvider;
        }

        public UpdateProvider()
        {

        }

        private List<UpdatePackage> GetAvailablePackages()
        {
            var updateList = new List<UpdatePackage>();
            var updateUrl = _configService.UpdateUrl;
            var rawUpdateList = _httpProvider.DownloadString(updateUrl);
            var matches = parseRegex.Matches(rawUpdateList);

            foreach (Match match in matches)
            {
                var updatePackage = new UpdatePackage();
                updatePackage.FileName = match.Groups["filename"].Value;
                updatePackage.Url = updateUrl + updatePackage.FileName;
                updatePackage.Version = new Version(match.Groups["version"].Value);
                updateList.Add(updatePackage);
            }

            return updateList;
        }

        public virtual UpdatePackage GetAvilableUpdate(Version currentVersion)
        {
            var latestAvailable = GetAvailablePackages().OrderByDescending(c => c.Version).FirstOrDefault();

            if (latestAvailable != null && latestAvailable.Version > currentVersion)
            {
                logger.Debug("An update is available ({0}) => ({1})", currentVersion, latestAvailable.Version);
                return latestAvailable;
            }

            logger.Trace("No updates available");
            return null;
        }

        public virtual Dictionary<DateTime, string> UpdateLogFile()
        {
            var list = new Dictionary<DateTime, string>();
            CultureInfo provider = CultureInfo.InvariantCulture;

            if (_diskProvider.FolderExists(_environmentProvider.GetUpdateLogFolder()))
            {
                var files = _diskProvider.GetFiles(_environmentProvider.GetUpdateLogFolder(), SearchOption.TopDirectoryOnly).ToList();

                foreach (var file in files.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name))
                {
                    list.Add(DateTime.ParseExact(file.Name.Replace(file.Extension, string.Empty), "yyyy.MM.dd-H-mm", provider), file.FullName);
                }
            }

            return list;
        }
    }
}
