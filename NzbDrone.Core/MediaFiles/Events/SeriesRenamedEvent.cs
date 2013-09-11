using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesRenamedEvent : IEvent
    {
        public Series Series { get; private set; }

        public SeriesRenamedEvent(Series series)
        {
            Series = series;
        }
    }
}