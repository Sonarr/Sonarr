using System;
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
        public PostDownloadStatusType PostDownloadStatus { get; set; }
        public Nullable<Int32> AbsoluteEpisodeNumber { get; set; }
        public int SceneSeasonNumber { get; set; }
        public int SceneEpisodeNumber { get; set; }

        //Todo: This should be UTC
        public DateTime? GrabDate { get; set; }

        public bool HasFile
        {
            get { return EpisodeFile != null; }
        }
        
        public EpisodeStatuses Status
        {
            get
            {
                if (HasFile) return EpisodeStatuses.Ready;

                if (GrabDate != null)
                {
                    if (PostDownloadStatus == PostDownloadStatusType.Unpacking)
                        return EpisodeStatuses.Unpacking;

                    if (PostDownloadStatus == PostDownloadStatusType.Failed)
                        return EpisodeStatuses.Failed;

                    if (GrabDate.Value.AddDays(1) >= DateTime.Now)
                        return EpisodeStatuses.Downloading;
                }

                if (GrabDate != null && GrabDate.Value.AddDays(1) >= DateTime.Now)
                    return EpisodeStatuses.Downloading;

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

        public Series Series { get; set; }

        public EpisodeFile EpisodeFile { get; set; }

        public override string ToString()
        {
            string seriesTitle = Series == null ? "[NULL]" : Series.Title;

            if (Series != null && Series.SeriesType == SeriesTypes.Daily && AirDate.HasValue)
                return string.Format("{0} - {1:yyyy-MM-dd}", seriesTitle, AirDate.Value);

            return string.Format("{0} - S{1:00}E{2:00}", seriesTitle, SeasonNumber, EpisodeNumber);
        }
    }
}