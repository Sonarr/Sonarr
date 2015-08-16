using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFolderCreatedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public string MovieFolder { get; set; }

        public MovieFolderCreatedEvent(Movie movie, MovieFile movieFile)
        {
            Movie = movie;
            MovieFile = movieFile;
        }
    }
}
