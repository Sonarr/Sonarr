using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseResource : RestResource
    {
        public QualityModel Quality { get; set; }
        public Int32 Age { get; set; }
        public Int64 Size { get; set; }
        public String Indexer { get; set; }
        public String ReleaseGroup { get; set; }
        public String Title { get; set; }
        public Boolean FullSeason { get; set; }
        public Boolean SceneSource { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Language Language { get; set; }
        public DateTime? AirDate { get; set; }
        public String SeriesTitle { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public Boolean Approved { get; set; }
        public Int32 TvRageId { get; set; }
        public List<string> Rejections { get; set; }
        public DateTime PublishDate { get; set; }
        public String CommentUrl { get; set; }
        public String DownloadUrl { get; set; }
        public String InfoUrl { get; set; }
        public Boolean DownloadAllowed { get; set; }
    }
}