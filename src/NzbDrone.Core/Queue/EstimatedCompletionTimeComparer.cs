using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Queue
{
    public class EstimatedCompletionTimeComparer : IComparer<DateTime?>
    {
        public int Compare(DateTime? x, DateTime? y)
        {
            if (!x.HasValue && !y.HasValue)
            {
                return 0;
            }

            if (!x.HasValue && y.HasValue)
            {
                return 1;
            }

            if (x.HasValue && !y.HasValue)
            {
                return -1;
            }

            if (x.Value > y.Value)
            {
                return 1;
            }

            if (x.Value < y.Value)
            {
                return -1;
            }

            return 0;
        }
    }
}
