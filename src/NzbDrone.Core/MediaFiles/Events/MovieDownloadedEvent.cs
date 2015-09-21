using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieDownloadedEvent : IEvent
    {
        public LocalMovie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public MovieFile OldFile { get; private set; }

        public MovieDownloadedEvent(LocalMovie movie, MovieFile movieFile, MovieFile oldFile)
        {
            Movie = movie;
            MovieFile = movieFile;
            OldFile = oldFile;
        }
    }
}