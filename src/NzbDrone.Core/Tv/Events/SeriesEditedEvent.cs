using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesEditedEvent : IEvent
    {
        public Series Series { get; private set; }
        public Series OldSeries { get; private set; }

        public SeriesEditedEvent(Series series, Series oldSeries)
        {
            Series = series;
            OldSeries = oldSeries;
        }
    }
}