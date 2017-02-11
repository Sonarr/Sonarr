using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Queue
{
    public class TimeleftComparer : IComparer<TimeSpan?>
    {
        public int Compare(TimeSpan? x, TimeSpan? y)
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
