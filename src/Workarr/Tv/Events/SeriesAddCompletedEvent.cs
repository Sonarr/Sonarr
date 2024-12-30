using Workarr.Messaging;

namespace Workarr.Tv.Events
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
