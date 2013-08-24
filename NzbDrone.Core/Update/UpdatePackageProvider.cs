using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate();
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public UpdatePackageProvider(IConfigFileProvider configFileProvider, IHttpProvider httpProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public UpdatePackage GetLatestUpdate()
        {
            var url = String.Format("{0}/v1/update/{1}?{2}", Services.RootUrl, _configFileProvider.Branch, BuildInfo.Version);
            var update = JsonConvert.DeserializeObject<UpdatePackageAvailable>(_httpProvider.DownloadString(url));

            if (!update.Available) return null;

            return update.UpdatePackage;
        }
    }
}