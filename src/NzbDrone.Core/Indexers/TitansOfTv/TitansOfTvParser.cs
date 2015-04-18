using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvParser : IParseIndexerResponse
    {
        public IList<Parser.Model.ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var results = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK) return results;


            var content = indexerResponse.HttpResponse.Content;

            var parsed = JsonConvert.DeserializeObject<ApiResult>(content);

            foreach (var parsedItem in parsed.results)
            {
                var release = new TorrentInfo();
                release.DownloadUrl = parsedItem.download;
                release.DownloadProtocol=DownloadProtocol.Torrent;
                release.Guid = parsedItem.download;
                release.Title = parsedItem.release_name;
                release.Size = Convert.ToInt64(parsedItem.size);
                release.Indexer = "ToTV";
                release.Seeders = Convert.ToInt32(parsedItem.seeders);
                release.Peers = Convert.ToInt32(parsedItem.leechers) + release.Seeders;
                release.PublishDate=  new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime().AddSeconds(parsedItem.created_at);
               results.Add(release);

            }
            return results;
        }
    }
}
