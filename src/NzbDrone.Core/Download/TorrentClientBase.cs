using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MonoTorrent;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Configuration;
using NLog;

namespace NzbDrone.Core.Download
{
    public abstract class TorrentClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected readonly IHttpProvider _httpProvider;
        protected readonly ITorrentFileInfoReader _torrentFileInfoReader;

        protected TorrentClientBase(IHttpProvider httpProvider,
                                    ITorrentFileInfoReader torrentFileInfoReader,
                                    IConfigService configService,
                                    IDiskProvider diskProvider,
                                    IParsingService parsingService,
                                    Logger logger)
            : base(configService, diskProvider, parsingService, logger)
        {
            _httpProvider = httpProvider;
            _torrentFileInfoReader = torrentFileInfoReader;
        }
        
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Torrent;
            }
        }

        protected abstract String AddFromMagnetLink(String hash, String magnetLink);
        protected abstract String AddFromTorrentFile(String hash, String filename, Byte[] fileContent);

        public override String Download(RemoteEpisode remoteEpisode)
        {
            var torrentInfo = remoteEpisode.Release as TorrentInfo;

            if (torrentInfo != null && !torrentInfo.MagnetUrl.IsNullOrWhiteSpace())
            {
                var hash = new MagnetLink(torrentInfo.MagnetUrl).InfoHash.ToHex();

                var actualHash = AddFromMagnetLink(hash, torrentInfo.MagnetUrl);

                if (hash != actualHash)
                {
                    _logger.Warn("{0} did not return the expected InfoHash for '{1}', NzbDrone could potential lose track of the download in progress.", Definition.Implementation, torrentInfo.MagnetUrl);
                }

                return hash;
            }
            else if (remoteEpisode.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var hash = new MagnetLink(remoteEpisode.Release.DownloadUrl).InfoHash.ToHex();
                var actualHash = AddFromMagnetLink(hash, remoteEpisode.Release.DownloadUrl);

                if (hash != actualHash)
                {
                    _logger.Warn("{0} did not return the expected InfoHash for '{1}', NzbDrone could potential lose track of the download in progress.", Definition.Implementation, remoteEpisode.Release.DownloadUrl);
                }

                return hash;
            }
            else
            {
                var filename = String.Format("{0}.torrent", FileNameBuilder.CleanFileName(remoteEpisode.Release.Title));

                // If the torrent file is a regular link, let NzbDrone download the file and send the file contents to the download client
                using (var torrent = _httpProvider.DownloadStream(remoteEpisode.Release.DownloadUrl))
                {
                    var torrentFile = torrent.ToBytes();

                    var hash = _torrentFileInfoReader.GetHashFromTorrentFile(torrentFile);
                    var actualHash = AddFromTorrentFile(hash, filename, torrentFile);
                    
                    if (hash != actualHash)
                    {
                        _logger.Warn("{0} did not return the expected InfoHash for '{1}', NzbDrone could potential lose track of the download in progress.", Definition.Implementation, filename);
                    }

                    return hash;
                }
            }
        }


    }
}
