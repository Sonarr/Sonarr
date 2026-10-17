using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(SimklParser));

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var series = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return series;
            }

            var jsonResponse = Json.Deserialize<SimklResponse>(_importResponse.Content);

            // no shows were returned
            if (jsonResponse == null)
            {
                return series;
            }

            if (jsonResponse.Anime != null)
            {
                foreach (var show in jsonResponse.Anime)
                {
                    if (show.AnimeType is not (SimklAnimeType.Tv or SimklAnimeType.Ona or SimklAnimeType.Ova or SimklAnimeType.Special))
                    {
                        Logger.Warn("Skipping info grabbing for '{0}' because it is an unsupported content type.", show.Show.Title);
                        continue;
                    }

                    var item = MapSeries(show);

                    if (item.TvdbId <= 0 && item.ImdbId.IsNullOrWhiteSpace() && item.TmdbId <= 0 && item.MalId <= 0)
                    {
                        Logger.Warn("Skipping info grabbing for '{0}' because it has no supported IDs.", show.Show.Title);
                        continue;
                    }

                    series.Add(item);
                }
            }

            if (jsonResponse.Shows != null)
            {
                foreach (var show in jsonResponse.Shows)
                {
                    series.Add(MapSeries(show));
                }
            }

            return series;
        }

        private static ImportListItemInfo MapSeries(SimklSeriesResource show)
        {
            var ids = show.Show.Ids;

            return new ImportListItemInfo
            {
                Title = show.Show.Title,
                TvdbId = int.TryParse(ids.Tvdb, out var tvdbId) ? tvdbId : 0,
                ImdbId = ids.Imdb,
                TmdbId = int.TryParse(ids.Tmdb, out var tmdbId) ? tmdbId : 0,
                MalId = int.TryParse(ids.Mal, out var malId) ? malId : 0
            };
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Simkl API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Simkl API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
