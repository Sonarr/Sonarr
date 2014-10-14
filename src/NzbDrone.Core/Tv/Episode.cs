using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Common;

namespace NzbDrone.Core.Tv
{
    public class Episode : ModelBase
    {
        public Episode()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public const string AIR_DATE_FORMAT = "yyyy-MM-dd";

        public int SeriesId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public string Overview { get; set; }
        public Boolean Monitored { get; set; }
        public Nullable<Int32> AbsoluteEpisodeNumber { get; set; }
        public Nullable<Int32> SceneAbsoluteEpisodeNumber { get; set; }
        public Nullable<Int32> SceneSeasonNumber { get; set; }
        public Nullable<Int32> SceneEpisodeNumber { get; set; }
        public Ratings Ratings { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }

        public String SeriesTitle { get; private set; }

        public LazyLoaded<EpisodeFile> EpisodeFile { get; set; }

        public Series Series { get; set; }

        public Boolean HasFile
        {
            get { return EpisodeFileId > 0; }
        }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", Id, Title.NullSafe());
        }
    }
}