using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser
{
    public static class SeasonNumbersFormatter
    {
        public static string Format(IEnumerable<int> seasonNumbers, string numberFormat = "0")
        {
            var seasons = seasonNumbers.Distinct().Order().ToList();

            if (seasons.Count == 1)
            {
                return $"Season {seasons[0].ToString(numberFormat)}";
            }

            if (seasons.Last() - seasons.First() + 1 == seasons.Count)
            {
                return $"Seasons {seasons.First().ToString(numberFormat)}-{seasons.Last().ToString(numberFormat)}";
            }

            return $"Seasons {string.Join(", ", seasons.Select(s => s.ToString(numberFormat)))}";
        }
    }
}
