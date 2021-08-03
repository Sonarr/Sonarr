using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetParser : IParseIndexerResponse
    {
        private static readonly Regex RegexProtocol = new Regex("^https?:", RegexOptions.Compiled);

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();

            switch (indexerResponse.HttpResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new ApiKeyException("API Key invalid or not authorized");
                case HttpStatusCode.NotFound:
                    throw new IndexerException(indexerResponse, "Indexer API call returned NotFound, the Indexer API may have changed.");
                case HttpStatusCode.ServiceUnavailable:
                    throw new RequestLimitReachedException("Cannot do more than 150 API requests per hour.");
                default:
                    if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
                    }

                    break;
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/html"))
            {
                throw new IndexerException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            }

            if (indexerResponse.Content == "Query execution was interrupted")
            {
                throw new IndexerException(indexerResponse, "Indexer API returned an internal server error");
            }

            JsonRpcResponse<BroadcastheNetTorrents> jsonResponse = new HttpResponse<JsonRpcResponse<BroadcastheNetTorrents>>(indexerResponse.HttpResponse).Resource;

            if (jsonResponse.Error != null || jsonResponse.Result == null)
            {
                throw new IndexerException(indexerResponse, "Indexer API call returned an error [{0}]", jsonResponse.Error);
            }

            if (jsonResponse.Result.Results == 0)
            {
                return results;
            }

            var protocol = indexerResponse.HttpRequest.Url.Scheme + ":";

            foreach (var torrent in jsonResponse.Result.Torrents.Values)
            {
                var torrentInfo = new TorrentInfo();

                torrentInfo.Guid = string.Format("BTN-{0}", torrent.TorrentID);
                torrentInfo.Title = CleanReleaseName(torrent.ReleaseName);
                torrentInfo.Size = torrent.Size;
                torrentInfo.DownloadUrl = RegexProtocol.Replace(torrent.DownloadURL, protocol);
                torrentInfo.InfoUrl = string.Format("{0}//broadcasthe.net/torrents.php?id={1}&torrentid={2}", protocol, torrent.GroupID, torrent.TorrentID);

                //torrentInfo.CommentUrl =
                if (torrent.TvdbID.HasValue)
                {
                    torrentInfo.TvdbId = torrent.TvdbID.Value;
                }

                if (torrent.TvrageID.HasValue)
                {
                    torrentInfo.TvRageId = torrent.TvrageID.Value;
                }

                torrentInfo.PublishDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime().AddSeconds(torrent.Time);

                //torrentInfo.MagnetUrl =
                torrentInfo.InfoHash = torrent.InfoHash;
                torrentInfo.Seeders = torrent.Seeders;
                torrentInfo.Peers = torrent.Leechers + torrent.Seeders;

                torrentInfo.Origin = torrent.Origin;
                torrentInfo.Source = torrent.Source;
                torrentInfo.Container = torrent.Container;
                torrentInfo.Codec = torrent.Codec;
                torrentInfo.Resolution = torrent.Resolution;

                results.Add(torrentInfo);
            }

            return results;
        }

        private string CleanReleaseName(string releaseName)
        {
            releaseName = releaseName.Replace("\\", "");

            return releaseName;
        }
    }
}
