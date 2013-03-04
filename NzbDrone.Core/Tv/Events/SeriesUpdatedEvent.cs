using System.Linq;
using NzbDrone.Common.Eventing;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesUpdatedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesUpdatedEvent(Series series)
        {
            Series = series;
        }
    }
}