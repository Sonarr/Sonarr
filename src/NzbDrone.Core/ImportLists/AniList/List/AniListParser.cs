using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.AniList.List
{
    public class AniListParser : IParseImportListResponse
    {
        private readonly Dictionary<int, MediaMapping> _mappings;
        private readonly Logger _logger;
        private readonly AniListSettings _settings;

        public AniListParser(Logger logger, AniListSettings settings, Dictionary<int, MediaMapping> mappings)
        {
            _mappings = mappings;
            _logger = logger;
            _settings = settings;
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

            // Filter out unwanted series status types
            var filtered = jsonResponse.Data.Page.MediaList
                .Where(x => ValidateMediaStatus(x.Media));

            foreach (var item in filtered)
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

        /// <summary>
        /// Filter by media status, based on user settings.
        ///
        /// Anilist currently does not support filtering this at the query level, so it must be done in post.
        /// </summary>
        private bool ValidateMediaStatus(MediaInfo media)
        {
            if (media.Status == MediaStatus.Finished && _settings.ImportFinished)
            {
                return true;
            }

            if (media.Status == MediaStatus.Releasing && _settings.ImportReleasing)
            {
                return true;
            }

            if (media.Status == MediaStatus.Unreleased && _settings.ImportUnreleased)
            {
                return true;
            }

            if (media.Status == MediaStatus.Cancelled && _settings.ImportCancelled)
            {
                return true;
            }

            if (media.Status == MediaStatus.Hiatus && _settings.ImportHiatus)
            {
                return true;
            }

            return false;
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
