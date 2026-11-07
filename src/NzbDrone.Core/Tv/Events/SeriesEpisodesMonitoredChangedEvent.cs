using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesEpisodesMonitoredChangedEvent : IEvent
    {
        public List<int> SeriesIds { get; private set; }

        public SeriesEpisodesMonitoredChangedEvent(List<int> seriesIds)
        {
            SeriesIds = seriesIds;
        }
    }
}
