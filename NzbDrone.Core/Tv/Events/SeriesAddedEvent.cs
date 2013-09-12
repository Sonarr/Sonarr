using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesAddedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesAddedEvent(Series series)
        {
            Series = series;
        }
    }
}