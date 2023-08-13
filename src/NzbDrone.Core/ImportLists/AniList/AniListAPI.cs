using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.ImportLists.AniList
{
    public enum AniListQuery
    {
        None,
        QueryList
    }

    public static class AniListAPI
    {
        // The maximum items anilist will return is 50 per request.
        // Therefore, pagination must be used to retrieve the full list.
        private const string QueryList = @"
            query ($id: String, $statusType: [MediaListStatus], $page: Int) {
                Page(page: $page) {
                    pageInfo {
                        currentPage
                        lastPage
                        hasNextPage
                    }
                    mediaList(userName: $id, type: ANIME, status_in: $statusType) {
                        status
                        progress
                        media {
                            id
                            format
                            title {
                                userPreferred
                                romaji
                            }
                            status
                            episodes
                            startDate {
                                year
                                month
                                day
                            }
                            endDate {
                                year
                                month
                                day
                            }
                        }
                    }
                }
            }
        ";

        public static string BuildQuery(AniListQuery query, object data)
        {
            var querySource = "";
            if (query == AniListQuery.QueryList)
            {
                querySource = QueryList;
            }

            if (string.IsNullOrEmpty(querySource))
            {
                throw new Exception("Unknown Query Type");
            }

            var body = Json.ToJson(new
            {
                query = querySource,
                variables = data
            });
            return body;
        }
    }

    public class PageInfo
    {
        public int CurrentPage { get; set; }

        public int LastPage { get; set; }

        public bool HasNextPage { get; set; }
    }

    public class MediaTitles
    {
        public string UserPreferred { get; set; }

        public string UserRomaji { get; set; }
    }

    public class MediaInfo
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Format { get; set; }

        public int? Episodes { get; set; }

        public MediaTitles Title { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public YMDBlock StartDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public YMDBlock EndDate { get; set; }
    }

    public class MediaList
    {
        public MediaInfo Media { get; set; }

        public string Status { get; set; }

        public int Progress { get; set; }
    }

    public class MediaPage
    {
        public PageInfo PageInfo { get; set; }

        public List<MediaList> MediaList { get; set; }
    }

    public class MediaPageData
    {
        public MediaPage Page { get; set; }
    }

    public class MediaPageResponse
    {
        public MediaPageData Data { get; set; }
    }

    public class YMDBlock
    {
        public int? Year { get; set; }

        public int? Month { get; set; }

        public int? Day { get; set; }

        public DateTime? Convert()
        {
            if (Year == null)
            {
                return null;
            }

            return new DateTime((int)Year, Month ?? 1, Day ?? 1);
        }
    }
}
