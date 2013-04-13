using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Update
{
    public interface IAvailableUpdateService
    {
        IEnumerable<UpdatePackage> GetAvailablePackages();
    }

    public class AvailableUpdateService : IAvailableUpdateService
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;

        private static readonly Regex ParseRegex = new Regex(@"(?:\>)(?<filename>NzbDrone.+?(?<version>\d+\.\d+\.\d+\.\d+).+?)(?:\<\/A\>)", RegexOptions.IgnoreCase);

        public AvailableUpdateService(IConfigService configService, IHttpProvider httpProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
        }

        public IEnumerable<UpdatePackage> GetAvailablePackages()
        {
            var updateList = new List<UpdatePackage>();
            var updateUrl = _configService.UpdateUrl;
            var rawUpdateList = _httpProvider.DownloadString(updateUrl);
            var matches = ParseRegex.Matches(rawUpdateList);

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
    }
}