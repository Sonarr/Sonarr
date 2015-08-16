using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteMovie : RemoteItem
    {
        public Movie Movie { get; set; }

        public ParsedMovieInfo ParsedMovieInfo
        {
            get
            {
                return ParsedInfo as ParsedMovieInfo;
            }

            set
            {
                this.ParsedInfo = value;
            }
        }

        public bool IsRecentMovie()
        {
            return Movie.ReleaseDate >= DateTime.UtcNow.Date.AddDays(-14);
        }

        public override Media Media
        {
            get
            {
                return Movie;
            }
        }

        public override IEnumerable<Datastore.MediaModelBase> MediaFiles
        {
            get
            {
                if (Movie.MovieFileId > 0)
                    return new List<Datastore.MediaModelBase> { Movie.MovieFile.Value };
                return new List<Datastore.MediaModelBase>();
            }
        }
    }
}