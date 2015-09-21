using System;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class DownloadMovieMessage
    {
        public String Message { get; set; }
        public Movie Movie { get; set; }
        public MovieFile MovieFile { get; set; }
        public MovieFile OldFile { get; set; }
        public String SourcePath { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
