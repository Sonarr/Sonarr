using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Vuze
{
    public class Vuze : Transmission.Transmission
    {
        private const int MINIMAL_SUPPORTED_PROTOCOL_VERSION = 14;
        private readonly ITransmissionProxy _proxy;

        public Vuze(ITransmissionProxy proxy,
                    ITorrentFileInfoReader torrentFileInfoReader,
                    IHttpClient httpClient,
                    IConfigService configService,
                    IDiskProvider diskProvider,
                    IRemotePathMappingService remotePathMappingService,
                    Logger logger)
            : base(proxy, torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }

        protected override DownloadClientItem CreateDownloadItem(TransmissionTorrent torrent, OsPath outputPath)
        {
            // Vuze reports outputPath containing the torrent name already, so we need to subtract it
            // for the Transmission protocol compatibility

            if (outputPath.FullPath.EndsWith(torrent.Name, StringComparison.Ordinal))
            {
                var startOfTorrentName = outputPath.FullPath.LastIndexOf(torrent.Name, StringComparison.Ordinal);
                outputPath = new OsPath(outputPath.FullPath.Substring(0, startOfTorrentName));

                _logger.Debug("Vuze output directory: {0}", outputPath);
            }

            return base.CreateDownloadItem(torrent, outputPath);
        }

        protected override ValidationFailure ValidateVersion()
        {
            var versionString = _proxy.GetProtocolVersion(Settings);

            _logger.Debug("Vuze protocol version information: {0}", versionString);

            int version;
            if (!int.TryParse(versionString, out version) || version < MINIMAL_SUPPORTED_PROTOCOL_VERSION)
            {
                {
                    return new ValidationFailure(string.Empty, "Protocol version not supported, use Vuze 5.0.0.0 or higher with Vuze Web Remote plugin.");
                }
            }

            return null;
        }

        public override string Name
        {
            get
            {
                return "Vuze";
            }
        }
    }
}