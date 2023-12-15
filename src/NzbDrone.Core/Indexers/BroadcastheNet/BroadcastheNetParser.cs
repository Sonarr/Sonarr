using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetParser : IParseIndexerResponse
    {
        private static readonly Regex RegexProtocol = new ("^https?:", RegexOptions.Compiled);

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();
            var indexerHttpResponse = indexerResponse.HttpResponse;

            switch (indexerHttpResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new ApiKeyException("API Key invalid or not authorized");
                case HttpStatusCode.NotFound:
                    throw new IndexerException(indexerResponse, "Indexer API call returned NotFound, the Indexer API may have changed.");
                case HttpStatusCode.ServiceUnavailable:
                    throw new RequestLimitReachedException("Cannot do more than 150 API requests per hour.");
                default:
                    if (indexerHttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerHttpResponse.StatusCode);
                    }

                    break;
            }

            if (indexerHttpResponse.Headers.ContentType != null && indexerHttpResponse.Headers.ContentType.Contains("text/html"))
            {
                throw new IndexerException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            }

            if (indexerResponse.Content.ContainsIgnoreCase("Call Limit Exceeded"))
            {
                throw new RequestLimitReachedException("Cannot do more than 150 API requests per hour.");
            }

            if (indexerResponse.Content == "Query execution was interrupted")
            {
                throw new IndexerException(indexerResponse, "Indexer API returned an internal server error");
            }

            var jsonResponse = new HttpResponse<JsonRpcResponse<BroadcastheNetTorrents>>(indexerHttpResponse).Resource;

            if (jsonResponse.Error != null || jsonResponse.Result == null)
            {
                throw new IndexerException(indexerResponse, "Indexer API call returned an error [{0}]", jsonResponse.Error);
            }

            if (jsonResponse.Result.Results == 0 || jsonResponse.Result.Torrents?.Values == null)
            {
                return results;
            }

            var protocol = indexerResponse.HttpRequest.Url.Scheme + ":";

            foreach (var torrent in jsonResponse.Result.Torrents.Values)
            {
                var torrentInfo = new TorrentInfo
                {
                    Guid = $"BTN-{torrent.TorrentID}",
                    InfoUrl = $"{protocol}//broadcasthe.net/torrents.php?id={torrent.GroupID}&torrentid={torrent.TorrentID}",
                    DownloadUrl = RegexProtocol.Replace(torrent.DownloadURL, protocol),
                    Title = CleanReleaseName(torrent.ReleaseName),
                    InfoHash = torrent.InfoHash,
                    Size = torrent.Size,
                    Seeders = torrent.Seeders,
                    Peers = torrent.Leechers + torrent.Seeders,
                    PublishDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime().AddSeconds(torrent.Time),
                    Origin = torrent.Origin,
                    Source = torrent.Source,
                    Container = torrent.Container,
                    Codec = torrent.Codec,
                    Resolution = torrent.Resolution
                };

                if (torrent.TvdbID is > 0)
                {
                    torrentInfo.TvdbId = torrent.TvdbID.Value;
                }

                if (torrent.TvrageID is > 0)
                {
                    torrentInfo.TvRageId = torrent.TvrageID.Value;
                }

                results.Add(torrentInfo);
            }

            return results;
        }

        private string CleanReleaseName(string releaseName)
        {
            return releaseName.Replace("\\", "");
        }
    }
}
