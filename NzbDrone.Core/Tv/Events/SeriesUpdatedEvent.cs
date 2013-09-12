using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging;

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