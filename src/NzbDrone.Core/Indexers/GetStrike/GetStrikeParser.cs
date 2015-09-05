using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace NzbDrone.Core.Indexers.GetStrike
{
    public class GetStrikeParser : IParseIndexerResponse
    {
        private readonly GetStrikeSettings _settings;

        public GetStrikeParser(GetStrikeSettings settings)
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

            var jsonResponse = JsonConvert.DeserializeObject<GetStrikeResponse>(indexerResponse.Content);

            if (jsonResponse.statuscode != 200)
            {
                throw new IndexerException(indexerResponse,
                    "GetStrike API request returned status code {0} : {1}",
                    jsonResponse.statuscode,
                    indexerResponse.Content);
            }

            var responseData = jsonResponse.torrents as JArray;
            if (responseData == null)
            {
                throw new IndexerException(indexerResponse,
                    "Indexer API call response missing result data");
            }

            var queryResults = responseData.ToObject<GetStrikeQueryResponse[]>();
            var id = 0;

            foreach (var result in queryResults)
            {
                DateTime Added;
                try
                {
                    double unixTime = 0;
                    if (Double.TryParse(result.Added, out unixTime))
                    {
                        Added = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        Added = Added.AddSeconds(unixTime).ToLocalTime();
                    }
                    else
                    {
                        Added = DateTime.ParseExact(result.Added, "MMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                    }
                } catch (System.FormatException e)
                {
                    throw new IndexerException(indexerResponse,
                        "Bad date_time");
                }

                torrentInfos.Add(new TorrentInfo()
                {
                    Guid = string.Format("getStrike-{0}", id++),
                    Title = result.Title,
                    Size = result.Size,
                    InfoHash = result.Hash,
                    MagnetUrl = result.MagnetUri,
                    InfoUrl = result.Info,
                    Seeders = result.Seeders,
                    Peers = result.Leechers + result.Seeders,
                    PublishDate = Added
                });
            }

            return torrentInfos.ToArray();
        }
    }
}
