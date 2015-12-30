using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTorrent;
using MonoTorrent.BEncoding;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Download
{
    public interface IResolveMagnetLink
    {
        byte[] DownloadTorrentFromMagnet(string magnetUrl);
    }

    public class MagnetResolverService : IResolveMagnetLink
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MagnetResolverService(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public byte[] DownloadTorrentFromMagnet(string magnetUrl)
        {
            var magnet = new MagnetLink(magnetUrl);

            var infoHash = magnet.InfoHash.ToHex();

            var request = new HttpRequest(string.Format("http://torcache.net/torrent/{0}.torrent", infoHash));

            _logger.Debug("Fetching torrent from torcache {0}", infoHash);

            try
            {
                var response = _httpClient.Get(request);

                if (response.Headers.ContentType != "application/x-bittorrent")
                {
                    _logger.Debug("Torcache returned unexpected ContentType: {0}", response.Headers.ContentType);
                    return null;
                }

                var data = response.ResponseData;

                if (magnet.AnnounceUrls.Any())
                {
                    var dictionary = (BEncodedDictionary)BEncodedValue.Decode(data);

                    BEncodedValue announceList;
                    if (!dictionary.TryGetValue("announce-list", out announceList))
                    {
                        announceList = new BEncodedList();
                        dictionary.Add("announce-list", announceList);
                    }

                    var tiers = new RawTrackerTiers((BEncodedList)announceList);
                    tiers.Add(magnet.AnnounceUrls);

                    data = dictionary.Encode();
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.Debug("Failed to fetch torrent from torcache, using magnet instead. {0}", ex.Message);
                return null;
            }
        }
    }
}
