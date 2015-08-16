using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieDeletedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public bool DeleteFiles { get; private set; }

        public MovieDeletedEvent(Movie movie,bool deleteFiles)
        {
            Movie = movie;
            DeleteFiles = deleteFiles;
        }
    }
}