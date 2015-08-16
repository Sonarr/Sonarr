using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScanSkippedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public ScanSkippedReason Reason { get; set; }

        public MovieScanSkippedEvent(Movie movie, ScanSkippedReason reason)
        {
            Movie = movie;
            Reason = reason;
        }
    }
}
