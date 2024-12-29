using NLog;
using Workarr.Extensions;
using Workarr.Instrumentation;
using Workarr.Parser.Model;
using Workarr.Serializer.Newtonsoft.Json;

namespace Workarr.ImportLists.MyAnimeList
{
    public class MyAnimeListParser : IParseImportListResponse
    {
        private static readonly Logger Logger = WorkarrLogger.GetLogger(typeof(MyAnimeListParser));

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
