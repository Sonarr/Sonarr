using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalMovie : LocalItem
    {
        public Movie Movie { get; set; }

        public override string ToString()
        {
            return Path;
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
                var returnValue = new List<Datastore.MediaModelBase>();
                if (Movie.MovieFileId > 0)
                    returnValue.Add(Movie.MovieFile.Value);
                return returnValue;
            }
        }

        public ParsedMovieInfo ParsedMovieInfo
        {
            get
            {
                return ParsedInfo as ParsedMovieInfo;
            }
            set
            {
                ParsedInfo = value;
            }
        }

        public override bool IsEmpty()
        {
            return false;
        }
    }
}