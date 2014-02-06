using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesEditedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesEditedEvent(Series series)
        {
            Series = series;
        }
    }
}