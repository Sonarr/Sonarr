using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesAddCompletedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesAddCompletedEvent(Series series)
        {
            Series = series;
        }
    }
}
