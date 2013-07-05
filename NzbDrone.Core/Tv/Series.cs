using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Common;


namespace NzbDrone.Core.Tv
{
    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public String AirTime { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public string Network { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }

        public string RootFolderPath { get; set; }

        public DateTime? FirstAired { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", Id, Title.NullSafe());
        }
    }
}