using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieRenamedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieRenamedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}