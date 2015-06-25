using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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
            var parsed = JsonConvert.DeserializeObject<ApiResult>(content);
            var protocol = indexerResponse.HttpRequest.Url.Scheme + ":";

            foreach (var parsedItem in parsed.results)
            {
                var release = new TorrentInfo();
                release.Guid = String.Format("ToTV-{0}", parsedItem.id);
                release.DownloadUrl = RegexProtocol.Replace(parsedItem.download, protocol);
                release.InfoUrl = RegexProtocol.Replace(parsedItem.episodeUrl, protocol);
                release.DownloadProtocol = DownloadProtocol.Torrent;
                release.Title = parsedItem.release_name;
                release.Size = Convert.ToInt64(parsedItem.size);
                release.Seeders = Convert.ToInt32(parsedItem.seeders);
                release.Peers = Convert.ToInt32(parsedItem.leechers) + release.Seeders;
                release.PublishDate = parsedItem.created_at;
                results.Add(release);
            }

            return results;
        }
    }
}
