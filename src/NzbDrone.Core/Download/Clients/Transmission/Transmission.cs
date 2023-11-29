using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class Transmission : TransmissionBase
    {
        public Transmission(ITransmissionProxy proxy,
                            ITorrentFileInfoReader torrentFileInfoReader,
                            IHttpClient httpClient,
                            IConfigService configService,
                            IDiskProvider diskProvider,
                            IRemotePathMappingService remotePathMappingService,
                            ILocalizationService localizationService,
                            IBlocklistService blocklistService,
                            Logger logger)
            : base(proxy, torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
        }

        protected override ValidationFailure ValidateVersion()
        {
            var versionString = _proxy.GetClientVersion(Settings);

            _logger.Debug("Transmission version information: {0}", versionString);

            var versionResult = Regex.Match(versionString, @"(?<!\(|(\d|\.)+)(\d|\.)+(?!\)|(\d|\.)+)").Value;
            var version = Version.Parse(versionResult);

            if (version < new Version(2, 40))
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion", new Dictionary<string, object> { { "clientName", Name }, { "requiredVersion", "2.40" }, { "reportedVersion", version } }));
            }

            return null;
        }

        public override string Name => "Transmission";
    }
}
