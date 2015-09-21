using System;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies
{
    public class MovieMovedEvent : IEvent
    {
        public Movie Movie { get; set; }
        public String SourcePath { get; set; }
        public String DestinationPath { get; set; }

        public MovieMovedEvent(Movie movie, string sourcePath, string destinationPath)
        {
            Movie = movie;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
