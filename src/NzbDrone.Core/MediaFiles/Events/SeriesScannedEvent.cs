using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesScannedEvent : IEvent
    {
        public Tv.Series Series { get; private set; }

        public SeriesScannedEvent(Tv.Series series)
        {
            Series = series;
        }
    }
}