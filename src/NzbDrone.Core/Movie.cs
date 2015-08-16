using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Movies
{
    public class Movie : ModelBase
    {
        public Movie()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string ImdbId { get; set; }
        public int TmdbId { get; set; }

        public string Title { get; set; }
        public int Year { get; set; }
        public string CleanTitle { get; set; }
        public string Overview { get; set; }
        public int Runtime { get; set; }
        public string TagLine { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public DateTime? LastInfoSync { get; set; }

        public string RootFolderPath { get; set; }

        public int QualityProfileId { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }

        public LazyLoaded<MovieFile> MovieFile { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ImdbId, Title.NullSafe());
        }

    }
}
