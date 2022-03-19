using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class FindSeriesResult
    {
        public Series Series { get; set; }
        public SeriesMatchType MatchType { get; set; }

        public FindSeriesResult(Series series, SeriesMatchType matchType)
        {
            Series = series;
            MatchType = matchType;
        }
    }

    public enum SeriesMatchType
    {
        Unknown = 0,
        Title = 1,
        Alias = 2,
        Id = 3
    }
}
