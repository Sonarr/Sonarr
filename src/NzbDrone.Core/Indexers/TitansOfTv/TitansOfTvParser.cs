using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvParser : IParseIndexerResponse
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
                    throw new RequestLimitReachedException("Indexer API is temporarily unavailable, try again later");
                default:
                    if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new IndexerException(indexerResponse, "Indexer API call returned an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
                    }
                    break;
            }

            var content = indexerResponse.HttpResponse.Content;
            var parsed = Json.Deserialize<TitansOfTvApiResult>(content);
            var protocol = indexerResponse.HttpRequest.Url.Scheme + ":";

            foreach (var parsedItem in parsed.results)
            {
                var release = new TorrentInfo();
                release.Guid = string.Format("ToTV-{0}", parsedItem.id);
                release.DownloadUrl = RegexProtocol.Replace(parsedItem.download, protocol);
                release.InfoUrl = RegexProtocol.Replace(parsedItem.episodeUrl, protocol);
                if (parsedItem.commentUrl.IsNotNullOrWhiteSpace())
                {
                    release.CommentUrl = RegexProtocol.Replace(parsedItem.commentUrl, protocol);
                }
                release.DownloadProtocol = DownloadProtocol.Torrent;
                release.Title = parsedItem.release_name;
                release.Size = parsedItem.size;
                release.Seeders = parsedItem.seeders;
                release.Peers = parsedItem.leechers + release.Seeders;
                release.PublishDate = parsedItem.created_at;
                results.Add(release);
            }

            return results;
        }
    }
}
