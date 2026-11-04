using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListParser : IParseImportListResponse
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MDBListParser));

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            var series = new List<ImportListItemInfo>();

            switch (importListResponse.HttpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.Unauthorized:
                    throw new ImportListException(importListResponse, "MDBList sign in has expired or was revoked, authenticate with MDBList again");
                case HttpStatusCode.Forbidden:
                    throw new ImportListException(importListResponse, "MDBList refused the request: {0}. Authenticate with MDBList again if this persists", GetError(importListResponse) ?? "access denied");
                case HttpStatusCode.NotFound:
                    throw new ImportListException(importListResponse, "MDBList list not found, check the list URL. Private lists can only be read by their owner");
                default:
                    throw new ImportListException(importListResponse, "MDBList API call resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            var jsonResponse = Json.Deserialize<MDBListItemsResponse>(importListResponse.Content);

            if (jsonResponse?.Shows == null)
            {
                return series;
            }

            foreach (var show in jsonResponse.Shows)
            {
                var item = new ImportListItemInfo
                {
                    Title = show.Title,
                    Year = show.ReleaseYear ?? 0,
                    TvdbId = show.Ids?.Tvdb ?? show.TvdbId ?? 0,
                    TmdbId = show.Ids?.Tmdb ?? 0,
                    ImdbId = show.Ids?.Imdb ?? show.ImdbId
                };

                if (item.TvdbId <= 0 && item.TmdbId <= 0 && item.ImdbId.IsNullOrWhiteSpace())
                {
                    Logger.Warn("Skipping '{0}' because it has no supported IDs.", show.Title);
                    continue;
                }

                series.Add(item);
            }

            return series;
        }

        private static string GetError(ImportListResponse importListResponse)
        {
            try
            {
                return Json.Deserialize<MDBListErrorResponse>(importListResponse.Content)?.Error;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
