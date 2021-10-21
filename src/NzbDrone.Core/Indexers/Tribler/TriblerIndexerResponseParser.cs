using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using Tribler.Api;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class TriblerIndexerResponseParser : IParseIndexerResponse
    {
        private readonly long TORRENT_INVENTION_UNIX_TIMESTAMP = 946684800;

        private readonly TriblerIndexerSettings _settings;

        public TriblerIndexerResponseParser(TriblerIndexerSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(indexerResponse.Content);


            if (searchResponse.Results == null || searchResponse.Results.Count == 0)
            {
                throw new IndexerException(indexerResponse,
                    "Indexer API call response missing result data");
            }

            foreach (var torrent in searchResponse.Results)
            {
                // currently i have no idea how to fill out TvdbId, TvRageId, ImdbId
                var release = new TorrentInfo
                {
                    Title = torrent.Name,
                    Seeders = torrent.Num_seeders,
                    Peers = torrent.Num_leechers,
                    InfoHash = torrent.Infohash,
                    DownloadProtocol = DownloadProtocol.Torrent,
                };

                torrentInfos.Add(release);

                if (torrent.Size.HasValue)
                {
                    release.Size = torrent.Size.Value;

                }

                if (torrent.Date.HasValue)
                {
                    release.PublishDate = DateTimeOffset.FromUnixTimeSeconds(torrent.Date.Value).DateTime;
                }
                else if (torrent.Updated.HasValue && torrent.Updated.Value > TORRENT_INVENTION_UNIX_TIMESTAMP) // drop invalid date's. 
                {
                    release.PublishDate = DateTimeOffset.FromUnixTimeSeconds(torrent.Updated.Value).DateTime;
                }

                release.MagnetUrl = string.Format("magnet:?xt=urn:btih:{0}&dn={1}", torrent.Infohash, torrent.Name);

                if (torrent.Size.HasValue)
                {
                    release.MagnetUrl += string.Format("&xl={0}", torrent.Size.Value);
                }

                release.DownloadUrl = release.MagnetUrl;

                release.Guid = "tribler-" + torrent.Id.GetValueOrDefault(0).ToString();
            }

            return torrentInfos;
        }

    }

}
