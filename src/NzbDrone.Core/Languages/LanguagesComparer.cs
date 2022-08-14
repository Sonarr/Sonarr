using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Languages
{
    public class LanguagesComparer : IComparer<List<Language>>
    {
        public int Compare(List<Language> x, List<Language> y)
        {
            if (!x.Any() && !y.Any())
            {
                return 0;
            }

            if (!x.Any() && y.Any())
            {
                return 1;
            }

            if (x.Any() && !y.Any())
            {
                return -1;
            }

            if (x.Count > 1 && y.Count > 1 && x.Count > y.Count)
            {
                return 1;
            }

            if (x.Count > 1 && y.Count > 1 && x.Count < y.Count)
            {
                return -1;
            }

            if (x.Count > 1 && y.Count == 1)
            {
                return 1;
            }

            if (x.Count == 1 && y.Count > 1)
            {
                return -1;
            }

            if (x.Count == 1 && y.Count == 1)
            {
                return x.First().Name.CompareTo(y.First().Name);
            }

            return 0;
        }
    }
}
