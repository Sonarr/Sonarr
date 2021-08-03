using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsParser : IParseIndexerResponse
    {
        private readonly HDBitsSettings _settings;

        public HDBitsParser(HDBitsSettings settings)
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

            var jsonResponse = JsonConvert.DeserializeObject<HDBitsResponse>(indexerResponse.Content);

            if (jsonResponse.Status != StatusCode.Success)
            {
                throw new IndexerException(indexerResponse,
                    "HDBits API request returned status code {0}: {1}",
                    jsonResponse.Status,
                    jsonResponse.Message ?? string.Empty);
            }

            var responseData = jsonResponse.Data as JArray;
            if (responseData == null)
            {
                throw new IndexerException(indexerResponse,
                    "Indexer API call response missing result data");
            }

            var queryResults = responseData.ToObject<TorrentQueryResponse[]>();

            foreach (var result in queryResults)
            {
                var id = result.Id;
                torrentInfos.Add(new TorrentInfo()
                {
                    Guid = string.Format("HDBits-{0}", id),
                    Title = result.Name,
                    Size = result.Size,
                    InfoHash = result.Hash,
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = result.Added.ToUniversalTime()
                });
            }

            return torrentInfos.ToArray();
        }

        private string GetDownloadUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/download.php")
                .AddQueryParam("id", torrentId)
                .AddQueryParam("passkey", _settings.ApiKey);

            return url.FullUri;
        }

        private string GetInfoUrl(string torrentId)
        {
            var url = new HttpUri(_settings.BaseUrl)
                .CombinePath("/details.php")
                .AddQueryParam("id", torrentId);

            return url.FullUri;
        }
    }
}
