using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieEditedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public Movie OldMovie { get; private set; }

        public MovieEditedEvent(Movie movie, Movie oldMovie)
        {
            Movie = movie;
            OldMovie = oldMovie;
        }
    }
}