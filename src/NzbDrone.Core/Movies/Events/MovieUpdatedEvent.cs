using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieUpdatedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieUpdatedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}