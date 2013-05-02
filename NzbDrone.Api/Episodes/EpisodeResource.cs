using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeResource : RestResource
    {
        public Int32 Id { get; set; }
        public Int32 SeriesId { get; set; }
        public Int32 EpisodeFileId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32 EpisodeNumber { get; set; }
        public String Title { get; set; }
        public DateTime? AirDate { get; set; }
        public EpisodeStatuses Status { get; set; }
        public String Overview { get; set; }
        public EpisodeFile EpisodeFile { get; set; }

        public Boolean HasFile { get; set; }
        public Boolean Ignored { get; set; }
        public Int32 SceneEpisodeNumber { get; set; }
        public Int32 SceneSeasonNumber { get; set; }
        public Int32 TvDbEpisodeId { get; set; }
        public Int32? AbsoluteEpisodeNumber { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? GrabDate { get; set; }
        public PostDownloadStatusType PostDownloadStatus { get; set; }
        public Core.Tv.Series Series { get; set; }
        public String SeriesTitle { get; set; }
    }
}
