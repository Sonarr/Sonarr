using Workarr.Messaging;

namespace Workarr.Tv.Events
{
    public class SeriesImportedEvent : IEvent
    {
        public List<int> SeriesIds { get; private set; }

        public SeriesImportedEvent(List<int> seriesIds)
        {
            SeriesIds = seriesIds;
        }
    }
}
