using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularRequestGenerator : TraktRequestGeneratorBase<TraktPopularSettings>
    {
        public TraktPopularRequestGenerator(TraktPopularSettings settings, string clientId, int pageSize, int maxNumResults)
            : base(settings, clientId, pageSize, maxNumResults)
        {
        }

        protected override void SetResource(HttpRequestBuilder requestBuilder)
        {
            var resource = "/shows";

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    resource += "/trending";
                    break;
                case (int)TraktPopularListType.Popular:
                    resource += "/popular";
                    break;
                case (int)TraktPopularListType.Anticipated:
                    resource += "/anticipated";
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    resource += "/watched/weekly";
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    resource += "/watched/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.TopWatchedByYear:
#pragma warning restore CS0612
                    resource += "/watched/yearly";
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    resource += "/watched/all";
                    break;
                case (int)TraktPopularListType.RecommendedByWeek:
                    resource += "/recommended/weekly";
                    break;
                case (int)TraktPopularListType.RecommendedByMonth:
                    resource += "/recommended/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.RecommendedByYear:
#pragma warning restore CS0612
                    resource += "/recommended/yearly";
                    break;
                case (int)TraktPopularListType.RecommendedByAllTime:
                    resource += "/recommended/all";
                    break;
            }

            requestBuilder.Resource(resource);
        }

        protected override Dictionary<string, string> GetFilterParameters()
        {
            return TraktQueryHelper.BuildFilterParameters(Settings.Rating, Settings.Genres, Settings.Years, _pageSize, Settings.TraktAdditionalParameters);
        }
    }
}
