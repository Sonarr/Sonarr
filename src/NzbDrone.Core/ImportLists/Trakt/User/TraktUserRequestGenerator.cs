using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : TraktRequestGeneratorBase<TraktUserSettings>
    {
        public TraktUserRequestGenerator(TraktUserSettings settings, string clientId, int pageSize, int maxNumResults)
            : base(settings, clientId, pageSize, maxNumResults)
        {
        }

        protected override void SetResource(HttpRequestBuilder requestBuilder)
        {
            var userName = Settings.Username.IsNotNullOrWhiteSpace() ? Settings.Username.Trim() : Settings.AuthUser.Trim();

            switch (Settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    var watchSorting = Settings.TraktWatchSorting switch
                    {
                        (int)TraktUserWatchSorting.Added => "added",
                        (int)TraktUserWatchSorting.Title => "title",
                        (int)TraktUserWatchSorting.Released => "released",
                        _ => "rank"
                    };

                    requestBuilder
                        .Resource("/users/{userName}/watchlist/shows/{watchSorting}")
                        .SetSegment("userName", userName)
                        .SetSegment("watchSorting", watchSorting);
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    requestBuilder
                        .Resource("/users/{userName}/watched/shows")
                        .SetSegment("userName", userName);
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    requestBuilder
                        .Resource("/users/{userName}/collection/shows")
                        .SetSegment("userName", userName);
                    break;
            }
        }

        protected override Dictionary<string, string> GetFilterParameters()
        {
            var filterParams = TraktQueryHelper.BuildFilterParameters(Settings.Rating, Settings.Genres, Settings.Years, _pageSize, Settings.TraktAdditionalParameters);

            if (Settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                filterParams["extended"] = "full";
            }

            return filterParams;
        }
    }
}
