using Workarr.Messaging;

namespace Workarr.Tv.Events
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
