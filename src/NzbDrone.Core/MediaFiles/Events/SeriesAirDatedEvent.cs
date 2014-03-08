using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesAirDatedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesAirDatedEvent(Series series)
        {
            Series = series;
        }
    }
}
