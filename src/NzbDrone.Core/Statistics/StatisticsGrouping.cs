using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Statistics
{
    public class StatisticsGrouping<T> where T : new()
    {
        public T LastWeek { get; set; }
        public T LastMonth { get; set; }
        public T AllTime { get; set; }

        public StatisticsGrouping()
        {
            LastWeek = new T();
            LastMonth = new T();
            AllTime = new T();
        }

        public void Apply(DateTime date, Action<T> applyAction)
        {
            var elapsed = DateTime.UtcNow - date.ToUniversalTime();

            if (elapsed < TimeSpan.FromDays(7))
            {
                applyAction(LastWeek);
            }

            if (elapsed < TimeSpan.FromDays(7 * 4))
            {
                applyAction(LastMonth);
            }

            applyAction(AllTime);
        }
    }
}
