using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.AniList
{
    public class AniListParser : IParseImportListResponse
    {
        private readonly Dictionary<int, MediaMapping> _mappings;
        private readonly Logger _logger;

        public AniListParser(Logger logger, Dictionary<int, MediaMapping> mappings)
        {
            _mappings = mappings;
            _logger = logger;
        }

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse)
        {
            return ParseResponse(importListResponse, out _);
        }

        public IList<ImportListItemInfo> ParseResponse(ImportListResponse importListResponse, out PageInfo pageInfo)
        {
            var result = new List<ImportListItemInfo>();

            if (!PreProcess(importListResponse))
            {
                pageInfo = null;
                return result;
            }

            var jsonResponse = STJson.Deserialize<MediaPageResponse>(importListResponse.Content);

            if (jsonResponse?.Data?.Page?.MediaList == null)
            {
                pageInfo = null;
                return result;
            }

            foreach (var item in jsonResponse.Data.Page.MediaList)
            {
                var media = item.Media;

                if (_mappings.TryGetValue(media.Id, out var mapping))
                {
                    // Base required data
                    var entry = new ImportListItemInfo()
                    {
                        TvdbId = mapping.TVDB.Value,
                        Title = media.Title.UserPreferred ?? media.Title.UserRomaji ?? default
                    };

                    // Extra optional mappings
                    if (mapping.MyAnimeList.HasValue)
                    {
                        entry.MalId = mapping.MyAnimeList.Value;
                    }

                    if (!string.IsNullOrEmpty(mapping.IMDB))
                    {
                        entry.ImdbId = mapping.IMDB;
                    }

                    // Optional Year/ReleaseDate data
                    if (media.StartDate?.Year != null)
                    {
                        entry.Year = (int)media.StartDate.Year;
                        entry.ReleaseDate = (System.DateTime)media.StartDate.Convert();
                    }

                    result.AddIfNotNull(entry);
                }
                else
                {
                    _logger.Warn("'{1}' (id:{0}) could not be imported, because there is no mapping available.", media.Id, media.Title.UserPreferred ?? media.Title.UserRomaji);
                }
            }

            pageInfo = jsonResponse.Data.Page.PageInfo;
            return result;
        }

        protected virtual bool PreProcess(ImportListResponse netImportResponse)
        {
            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(netImportResponse, "Anilist API call resulted in an unexpected StatusCode [{0}]", netImportResponse.HttpResponse.StatusCode);
            }

            if (netImportResponse.HttpResponse.Headers.ContentType != null && !netImportResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                netImportResponse.HttpRequest.Headers.Accept != null && netImportResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new ImportListException(netImportResponse, "Anilist API responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
