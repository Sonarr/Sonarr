using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalParser : IParseImportListResponse
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MalParser));

        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var jsonResponse = Json.Deserialize<MalResponse>(importListResponse.Content);

            var series = new List<ImportListItemInfo>();

            foreach (var show in jsonResponse.Animes)
            {
                var tentativeTvdbId = MalImport.MalTvdbIds.TryGetValue(show.AnimeInfo.Id, out var a) ? a : -1;

                if (tentativeTvdbId > 0)
                {
                    series.AddIfNotNull(new ImportListItemInfo
                    {
                        Title = show.AnimeInfo.Title,
                        TvdbId = tentativeTvdbId,
                        MalId = show.AnimeInfo.Id
                    });
                }
                else
                {
                    Logger.Warn($"No TVDB ID associated with {show.AnimeInfo.Title} ({show.AnimeInfo.Id}), skipping");
                }
            }

            return series;
        }
    }
}
