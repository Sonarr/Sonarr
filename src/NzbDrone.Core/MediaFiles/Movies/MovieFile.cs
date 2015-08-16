using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;
using System;

namespace NzbDrone.Core.MediaFiles.Movies
{
    public class MovieFile : MediaModelBase
    {
        // Movies
        public Int32 MovieId { get; set; }
        public LazyLoaded<Movie> Movie { get; set; }
    }
}