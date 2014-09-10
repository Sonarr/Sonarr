using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Common;

namespace NzbDrone.Core.Tv
{
    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<String>();
            Actors = new List<Actor>();
            Tags = new List<Int32>();
        }

        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public String AirTime { get; set; }
        public bool Monitored { get; set; }
        public int ProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public string Network { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<String> Genres { get; set; }
        public List<Actor> Actors { get; set; }
        public String Certification { get; set; }

        public string RootFolderPath { get; set; }

        public DateTime? FirstAired { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }

        public List<Season> Seasons { get; set; }
        public List<Int32> Tags { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", TvdbId, Title.NullSafe());
        }
    }
}