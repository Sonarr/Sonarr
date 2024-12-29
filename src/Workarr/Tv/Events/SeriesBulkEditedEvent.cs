using Workarr.Messaging;

namespace Workarr.Tv.Events
{
    public class SeriesBulkEditedEvent : IEvent
    {
        public List<Series> Series { get; private set; }

        public SeriesBulkEditedEvent(List<Series> series)
        {
            Series = series;
        }
    }
}
