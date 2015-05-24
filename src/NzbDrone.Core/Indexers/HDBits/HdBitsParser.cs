using System;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using System.Net;
using NzbDrone.Core.Indexers.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Collections.Specialized;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HdBitsParser : IParseIndexerResponse
    {
        private readonly HdBitsSettings _settings;

        public HdBitsParser(HdBitsSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(
                    indexerResponse,
                    "Unexpected response status {0} code from API request",
                    indexerResponse.HttpResponse.StatusCode);
            }

            var jsonResponse = JsonConvert.DeserializeObject<HdBitsResponse>(indexerResponse.Content);

            if (jsonResponse.Status != StatusCode.Success)
            {
                throw new IndexerException(
                    indexerResponse,
                    @"HDBits API request returned status code {0} with message ""{1}""",
                    jsonResponse.Status,
                    jsonResponse.Message ?? "");
            }

            var responseData = jsonResponse.Data as JArray;
            if (responseData == null)
            {
                throw new IndexerException(
                    indexerResponse,
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
                    DownloadUrl = GetDownloadUrl(id),
                    InfoUrl = GetInfoUrl(id),
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = result.Added
                });
            }

            return torrentInfos.ToArray();
        }

        private string GetDownloadUrl(long torrentId)
        {
            var args = new NameValueCollection(2);
            args["id"] = torrentId.ToString();
            args["passkey"] = _settings.ApiKey;

            return BuildUrl("/download.php", args);
        }

        private string GetInfoUrl(long torrentId)
        {
            var args = new NameValueCollection(1);
            args["id"] = torrentId.ToString();

            return BuildUrl("/details.php", args);

        }
        
        private string BuildUrl(string path, NameValueCollection args)
        {
            var builder = new UriBuilder(_settings.BaseUrl);
            builder.Path = path;
            var queryString = HttpUtility.ParseQueryString("");

            queryString.Add(args);

            builder.Query = queryString.ToString();

            return builder.Uri.ToString();
        }
    }
}
