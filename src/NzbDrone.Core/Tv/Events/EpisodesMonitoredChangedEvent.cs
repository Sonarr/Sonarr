using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class EpisodesMonitoredChangedEvent : IEvent
    {
        public List<int> SeriesIds { get; private set; }

        public EpisodesMonitoredChangedEvent(List<int> seriesIds)
        {
            SeriesIds = seriesIds;
        }
    }
}
