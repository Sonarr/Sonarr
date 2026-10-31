using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public static class AmbiguousSeriesExtensions
    {
        public static bool IsAmbiguous(this List<Series> series)
        {
            return series is { Count: > 1 };
        }

        public static string ToAmbiguousSeriesMessage(this List<Series> series)
        {
            return $"Matches multiple series: {string.Join(", ", series.Select(s => s.Title))}";
        }
    }
}
