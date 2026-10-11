using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Notifications
{
    public static class SeasonNumbersFormatter
    {
        public static string Format(IEnumerable<int> seasonNumbers)
        {
            var seasons = seasonNumbers.Distinct().Order().ToList();

            if (seasons.Count == 1)
            {
                return $"Season {seasons[0]}";
            }

            if (seasons.Last() - seasons.First() + 1 == seasons.Count)
            {
                return $"Seasons {seasons.First()}-{seasons.Last()}";
            }

            return $"Seasons {string.Join(", ", seasons)}";
        }
    }
}
