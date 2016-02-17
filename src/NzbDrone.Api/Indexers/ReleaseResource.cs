using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Api.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseResource : RestResource
    {
        public string Guid { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public int Age { get; set; }
        public double AgeHours { get; set; }
        public double AgeMinutes { get; set; }
        public long Size { get; set; }
        public int IndexerId { get; set; }
        public string Indexer { get; set; }
        public string ReleaseGroup { get; set; }
        public string SubGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Title { get; set; }
        public bool FullSeason { get; set; }
        public bool SceneSource { get; set; }
        public int SeasonNumber { get; set; }
        public Language Language { get; set; }
        public string AirDate { get; set; }
        public string SeriesTitle { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public bool Approved { get; set; }
        public bool TemporarilyRejected { get; set; }
        public bool Rejected { get; set; }
        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public IEnumerable<string> Rejections { get; set; }
        public DateTime PublishDate { get; set; }
        public string CommentUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public bool DownloadAllowed { get; set; }
        public int ReleaseWeight { get; set; }


        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public DownloadProtocol Protocol { get; set; }


        // TODO: Remove in v3
        // Used to support the original Release Push implementation
        // JsonIgnore so we don't serialize it, but can still parse it
        [JsonIgnore]
        public DownloadProtocol DownloadProtocol
        {
            get
            {
                return Protocol;
            }
            set
            {
                if (value > 0 && Protocol == 0)
                {
                    Protocol = value;
                }
            }
        }

        public bool IsDaily { get; set; }
        public bool IsAbsoluteNumbering { get; set; }
        public bool IsPossibleSpecialEpisode { get; set; }
        public bool Special { get; set; }
    }
}