using System.Net;
using Workarr.Extensions;
using Workarr.ImportLists.Exceptions;
using Workarr.Parser.Model;
using Workarr.Serializer.System.Text.Json;

namespace Workarr.ImportLists.Trakt
{
    public class TraktParser : IParseImportListResponse
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

            var traktResponses = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content);

            // no series were returned
            if (traktResponses == null)
            {
                return series;
            }

            foreach (var traktResponse in traktResponses)
            {
                series.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = traktResponse.Show.Title,
                    TvdbId = traktResponse.Show.Ids.Tvdb.GetValueOrDefault(),
                    ImdbId = traktResponse.Show.Ids.Imdb
                });
            }

            return series;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Trakt API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && !netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Trakt API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
