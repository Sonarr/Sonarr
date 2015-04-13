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
                var release = new ReleaseInfo();
                release.DownloadUrl = parsedItem.download;
                release.DownloadProtocol=DownloadProtocol.Torrent;
                release.Guid = parsedItem.download;
                release.Title = parsedItem.release_name;
                release.Size = Convert.ToInt64(parsedItem.size);
               results.Add(release);

            }
            return results;
        }
    }
}
