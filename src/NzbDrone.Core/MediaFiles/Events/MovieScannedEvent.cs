using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieScannedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieScannedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}