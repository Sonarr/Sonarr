using System.Linq;
using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Tv
{
    public class Episode
    {
        public int EpisodeId { get; set; }

        public int? TvDbEpisodeId { get; set; }

        public int SeriesId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }

        //Todo: Since we're displaying next airing relative to the user's timezone we may want to store this as UTC (with airtime + UTC offset)
        public DateTime? AirDate { get; set; }
        public string Overview { get; set; }
        public Boolean Ignored { get; set; }
        public PostDownloadStatusType PostDownloadStatus { get; set; }
        public int AbsoluteEpisodeNumber { get; set; }
        public int SceneSeasonNumber { get; set; }
        public int SceneEpisodeNumber { get; set; }

        //Todo: This should be UTC
        public DateTime? GrabDate { get; set; }

        public EpisodeStatusType Status
        {
            get
            {
                if (EpisodeFileId != 0) return EpisodeStatusType.Ready;

                if (GrabDate != null)
                {
                    if (PostDownloadStatus == PostDownloadStatusType.Unpacking)
                        return EpisodeStatusType.Unpacking;

                    if (PostDownloadStatus == PostDownloadStatusType.Failed)
                        return EpisodeStatusType.Failed;

                    if (GrabDate.Value.AddDays(1) >= DateTime.Now)
                        return EpisodeStatusType.Downloading;
                }

                if (GrabDate != null && GrabDate.Value.AddDays(1) >= DateTime.Now)
                    return EpisodeStatusType.Downloading;

                if (AirDate != null && AirDate.Value.Date == DateTime.Today)
                    return EpisodeStatusType.AirsToday;

                if (AirDate != null && AirDate.Value.Date < DateTime.Now)
                    return EpisodeStatusType.Missing;

                return EpisodeStatusType.NotAired;
            }
        }

        public Series Series { get; set; }

        public EpisodeFile EpisodeFile { get; set; }

        public override string ToString()
        {
            string seriesTitle = Series == null ? "[NULL]" : Series.Title;

            if (Series != null && Series.IsDaily && AirDate.HasValue)
                return string.Format("{0} - {1:yyyy-MM-dd}", seriesTitle, AirDate.Value);

            return string.Format("{0} - S{1:00}E{2:00}", seriesTitle, SeasonNumber, EpisodeNumber);
        }
    }
}