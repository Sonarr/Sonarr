using System.Runtime.Serialization;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public enum TraktPopularListType
    {
        [EnumMember(Value = "Trending Shows")]
        Trending = 0,
        [EnumMember(Value = "Popular Shows")]
        Popular = 1,
        [EnumMember(Value = "Anticipated Shows")]
        Anticipated = 2,

        [EnumMember(Value = "Top Watched Shows By Week")]
        TopWatchedByWeek = 3,
        [EnumMember(Value = "Top Watched Shows By Month")]
        TopWatchedByMonth = 4,
        [EnumMember(Value = "Top Watched Shows By Year")]
        TopWatchedByYear = 5,
        [EnumMember(Value = "Top Watched Shows Of All Time")]
        TopWatchedByAllTime = 6
    }
}
