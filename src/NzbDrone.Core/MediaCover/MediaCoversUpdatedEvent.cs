using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Series Series { get; set; }

        public MediaCoversUpdatedEvent(Series series)
        {
            Series = series;
        }
    }
}
