using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesRenamedEvent : IEvent
    {
        public Tv.Series Series { get; private set; }

        public SeriesRenamedEvent(Tv.Series series)
        {
            Series = series;
        }
    }
}