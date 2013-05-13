using System;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model;


namespace NzbDrone.Core.Tv
{
    public class Episode : ModelBase
    {
        public int TvDbEpisodeId { get; set; }
        public int SeriesId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime? AirDate { get; set; }

        public string Overview { get; set; }
        public Boolean Ignored { get; set; }
        public Nullable<Int32> AbsoluteEpisodeNumber { get; set; }
        public int SceneSeasonNumber { get; set; }
        public int SceneEpisodeNumber { get; set; }

        public bool HasFile
        {
            get { return EpisodeFileId != 0; }
        }

        public EpisodeStatuses Status
        {
            get
            {
                if (HasFile) return EpisodeStatuses.Ready;

                if (AirDate != null && AirDate.Value.Date == DateTime.Today)
                    return EpisodeStatuses.AirsToday;

                if (AirDate != null && AirDate.Value.Date < DateTime.Now)
                    return EpisodeStatuses.Missing;

                return EpisodeStatuses.NotAired;
            }
        }

        public DateTime? EndTime
        {
            get
            {
                if (!AirDate.HasValue) return null;
                if (Series == null) return null;

                return AirDate.Value.AddMinutes(Series.Runtime);
            }
        }

        public String SeriesTitle { get; private set; }

        public Series Series { get; set; }

        public LazyLoaded<EpisodeFile> EpisodeFile { get; set; }

        public override string ToString()
        {
            string seriesTitle = Series == null ? "[NULL]" : Series.Title;

            if (Series != null && Series.SeriesType == SeriesTypes.Daily && AirDate.HasValue)
                return string.Format("{0} - {1:yyyy-MM-dd}", seriesTitle, AirDate.Value);

            return string.Format("{0} - S{1:00}E{2:00}", seriesTitle, SeasonNumber, EpisodeNumber);
        }
    }
}