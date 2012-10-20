using System;
using NzbDrone.Core.Model;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("Episodes")]
    [PrimaryKey("EpisodeId", autoIncrement = true)]
    public class Episode
    {
        public int EpisodeId { get; set; }

        public int? TvDbEpisodeId { get; set; }

        public int SeriesId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime? AirDate { get; set; }
        public string Overview { get; set; }
        public Boolean Ignored { get; set; }
        public PostDownloadStatusType PostDownloadStatus { get; set; }
        public int AbsoluteEpisodeNumber { get; set; }
        public int SceneAbsoluteEpisodeNumber { get; set; }
        public int SceneSeasonNumber { get; set; }
        public int SceneEpisodeNumber { get; set; }

        /// <summary>
        /// Gets or sets the grab date.
        /// </summary>
        /// <remarks>
        /// Used to specify when the episode was grapped.
        /// this filed is used by status as an expirable "Grabbed" status.
        /// </remarks>
        public DateTime? GrabDate { get; set; }

        [ResultColumn]
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

        [ResultColumn]
        public Series Series { get; set; }

        [ResultColumn]
        public EpisodeFile EpisodeFile { get; set; }

        public override string ToString()
        {
            string seriesTitle = Series == null ? "[NULL]" : Series.Title;

            //if (IsDailyEpisode)
            //    return string.Format("{0} - {1}", seriesTitle, AirDate.Date);

            return string.Format("{0} - S{1:00}E{2:00}", seriesTitle, SeasonNumber, EpisodeNumber);
        }
    }
}