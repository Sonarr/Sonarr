using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Movies
{
    public class Movie : Media
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
            Tags = new HashSet<Int32>();
        }

        public string ImdbId { get; set; }
        public int TmdbId { get; set; }


        public string OriginalTitle { get; set; }

        public string TagLine { get; set; }
        public DateTime ReleaseDate { get; set; }

        public bool AddOptions { get; set; }

        public LazyLoaded<MovieFile> MovieFile { get; set; }
        public int MovieFileId { get; set; }
        public new LazyLoaded<Profile> Profile
        {
            get
            {
                return base.Profile;
            }
            set
            {
                base.Profile = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ImdbId, Title.NullSafe());
        }

    }
}
