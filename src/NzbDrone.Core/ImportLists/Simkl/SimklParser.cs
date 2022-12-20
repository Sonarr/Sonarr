using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklParser : IParseImportListResponse
    {
        private ImportListResponse _importResponse;

        public SimklParser()
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

            var jsonResponse = STJson.Deserialize<SimklResponse>(_importResponse.Content);

            // no shows were return
            if (jsonResponse == null)
            {
                return series;
            }

            foreach (var show in jsonResponse.Shows)
            {
                series.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = show.Show.Title,
                    TvdbId = int.TryParse(show.Show.Ids.Tvdb, out var tvdbId) ? tvdbId : 0,
                    ImdbId = show.Show.Ids.Imdb
                });
            }

            return series;
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
