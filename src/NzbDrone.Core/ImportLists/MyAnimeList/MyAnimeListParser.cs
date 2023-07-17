using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MyAnimeListParser : IParseImportListResponse
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MyAnimeListParser));

        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var jsonResponse = Json.Deserialize<MyAnimeListResponse>(importListResponse.Content);
            var series = new List<ImportListItemInfo>();

            foreach (var show in jsonResponse.Animes)
            {
                series.AddIfNotNull(new ImportListItemInfo
                {
                    Title = show.AnimeListInfo.Title,
                    MalId = show.AnimeListInfo.Id
                });
            }

            return series;
        }
    }
}
