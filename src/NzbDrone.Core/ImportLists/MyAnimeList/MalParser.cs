using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalParser : IParseImportListResponse
    {
        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var jsonResponse = Json.Deserialize<MalResponse>(importListResponse.Content);

            var series = new List<ImportListItemInfo>();

            foreach (var show in jsonResponse.Animes)
            {
                series.AddIfNotNull(new ImportListItemInfo
                {
                    Title = show.AnimeInfo.Title,
                    TvdbId = MalImport.Maltotvdb.TryGetValue(show.AnimeInfo.Id, out var a) ? a : -1
                });
            }

            return series;
        }
    }
}
