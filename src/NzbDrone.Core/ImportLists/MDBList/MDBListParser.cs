using System.Collections.Generic;
using System.Linq;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var series = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return series;
            }

            var items = ParseItems(_importResponse.Content);

            foreach (var item in items.Where(IsShow))
            {
                series.AddIfNotNull(new ImportListItemInfo
                {
                    Title = item.Title,
                    TvdbId = GetTvdbId(item),
                    TmdbId = item.Ids?.Tmdb ?? 0,
                    ImdbId = item.ImdbId.IsNotNullOrWhiteSpace() ? item.ImdbId : item.Ids?.Imdb,
                    Year = item.ReleaseYear.GetValueOrDefault()
                });
            }

            return series;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "MDBList API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/html") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/html"))
            {
                throw new ImportListException(importListResponse, "MDBList API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

        private static List<MDBListItemResource> ParseItems(string content)
        {
            var trimmed = content.TrimStart();

            if (trimmed.StartsWith("["))
            {
                return Json.Deserialize<List<MDBListItemResource>>(content) ?? new List<MDBListItemResource>();
            }

            var response = Json.Deserialize<MDBListResponse>(content);

            return response?.Shows ?? new List<MDBListItemResource>();
        }

        private static int GetTvdbId(MDBListItemResource item)
        {
            return item.TvdbId.GetValueOrDefault(item.LegacyTvdbId.GetValueOrDefault(item.Ids?.Tvdb ?? 0));
        }

        private static bool IsShow(MDBListItemResource item)
        {
            return item.Mediatype.IsNullOrWhiteSpace() || item.Mediatype.Equals("show", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
