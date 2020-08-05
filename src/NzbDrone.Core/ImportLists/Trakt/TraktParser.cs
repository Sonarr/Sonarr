using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public TraktParser()
        {
        }

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var series = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return series;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<TraktResponse>>(_importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return series;
            }

            foreach (var movie in jsonResponse)
            {
                series.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = movie.Show.Title,
                    TvdbId = movie.Show.Ids.Tvdb.GetValueOrDefault()
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
