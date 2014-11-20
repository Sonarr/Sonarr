using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetParser : IParseIndexerResponse
    {
        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new ApiKeyException("API Key invalid or not authorized");
            }
            else if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new IndexerException(indexerResponse, "Indexer API call returned NotFound, the Indexer API may have changed.");
            }
            else if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new RequestLimitReachedException("Cannot do more than 150 API requests per hour.");
            }
            else if (indexerResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            var jsonResponse = new HttpResponse<JsonRpcResponse<BroadcastheNetTorrents>>(indexerResponse.HttpResponse).Resource;

            if (jsonResponse.Error != null || jsonResponse.Result == null)
            {
                throw new IndexerException(indexerResponse, "Indexer API call returned an error [{0}]", jsonResponse.Error);
            }
            
            if (jsonResponse.Result.Results == 0)
            {
                return results;
            }

            foreach (var torrent in jsonResponse.Result.Torrents.Values)
            {
                var torrentInfo = new TorrentInfo();

                torrentInfo.Guid = String.Format("BTN-{0}", torrent.TorrentID.ToString());
                torrentInfo.Title = torrent.ReleaseName;
                torrentInfo.Size = torrent.Size;
                torrentInfo.DownloadUrl = torrent.DownloadURL;
                torrentInfo.InfoUrl = String.Format("https://broadcasthe.net/torrents.php?id={0}&torrentid={1}", torrent.GroupID, torrent.TorrentID);
                //torrentInfo.CommentUrl =
                if (torrent.TvrageID.HasValue)
                {
                    torrentInfo.TvRageId = torrent.TvrageID.Value;
                }
                torrentInfo.PublishDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime().AddSeconds(torrent.Time);
                //torrentInfo.MagnetUrl = 
                torrentInfo.InfoHash = torrent.InfoHash;
                torrentInfo.Seeds = torrent.Seeders;
                torrentInfo.Peers = torrent.Leechers;

                results.Add(torrentInfo);
            }

            return results;
        }
    }
}
