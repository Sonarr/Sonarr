using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class SeriesRefreshStartingEvent : IEvent
    {
        public bool ManualTrigger { get; set; }

        public SeriesRefreshStartingEvent(bool manualTrigger)
        {
            ManualTrigger = manualTrigger;
        }
    }
}
