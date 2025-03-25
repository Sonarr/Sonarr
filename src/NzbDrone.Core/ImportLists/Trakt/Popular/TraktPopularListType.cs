using System;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public enum TraktPopularListType
    {
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTrendingShows")]
        Trending = 0,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypePopularShows")]
        Popular = 1,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeAnticipatedShows")]
        Anticipated = 2,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopWeekShows")]
        TopWatchedByWeek = 3,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopMonthShows")]
        TopWatchedByMonth = 4,

        [Obsolete]
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopYearShows")]
        TopWatchedByYear = 5,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeTopAllTimeShows")]
        TopWatchedByAllTime = 6,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedWeekShows")]
        RecommendedByWeek = 7,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedMonthShows")]
        RecommendedByMonth = 8,

        [Obsolete]
        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedYearShows")]
        RecommendedByYear = 9,

        [FieldOption(Label = "ImportListsTraktSettingsPopularListTypeRecommendedAllTimeShows")]
        RecommendedByAllTime = 10
    }
}
