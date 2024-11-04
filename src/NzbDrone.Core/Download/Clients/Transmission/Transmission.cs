using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
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
        public override string Name => "Transmission";
        public override bool SupportsLabels => HasClientVersion(4, 0);

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

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            if (!SupportsLabels)
            {
                throw new NotSupportedException($"{Name} does not support marking items as imported");
            }

            // set post-import category
            if (Settings.TvImportedCategory.IsNotNullOrWhiteSpace() &&
                Settings.TvImportedCategory != Settings.TvCategory)
            {
                var hash = downloadClientItem.DownloadId.ToLowerInvariant();
                var torrent = _proxy.GetTorrents(new[] { hash }, Settings).FirstOrDefault();

                if (torrent == null)
                {
                    _logger.Warn("Could not find torrent with hash \"{0}\" in Transmission.", hash);
                    return;
                }

                try
                {
                    var labels = torrent.Labels.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
                    labels.Add(Settings.TvImportedCategory);

                    if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                    {
                        labels.Remove(Settings.TvCategory);
                    }

                    _proxy.SetTorrentLabels(hash, labels, Settings);
                }
                catch (DownloadClientException ex)
                {
                    _logger.Warn(ex, "Failed to set post-import torrent label \"{0}\" for {1} in Transmission.", Settings.TvImportedCategory, downloadClientItem.Title);
                }
            }
        }

        protected override ValidationFailure ValidateVersion()
        {
            var versionString = _proxy.GetClientVersion(Settings, true);

            _logger.Debug("Transmission version information: {0}", versionString);

            var versionResult = Regex.Match(versionString, @"(?<!\(|(\d|\.)+)(\d|\.)+(?!\)|(\d|\.)+)").Value;
            var version = Version.Parse(versionResult);

            if (version < new Version(2, 40))
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion", new Dictionary<string, object> { { "clientName", Name }, { "requiredVersion", "2.40" }, { "reportedVersion", version } }));
            }

            return null;
        }
    }
}
