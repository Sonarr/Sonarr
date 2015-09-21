using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class SeriesScanSkippedEvent : IEvent
    {
        public Tv.Series Series { get; private set; }
        public ScanSkippedReason Reason { get; set; }

        public SeriesScanSkippedEvent(Tv.Series series, ScanSkippedReason reason)
        {
            Series = series;
            Reason = reason;
        }
    }
}
