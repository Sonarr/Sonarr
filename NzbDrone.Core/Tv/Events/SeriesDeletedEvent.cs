using System.Linq;
using NzbDrone.Common.Eventing;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesDeletedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesDeletedEvent(Series series)
        {
            Series = series;
        }
    }
}