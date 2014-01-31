using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeFileResource : RestResource
    {
        public Int32 SeriesId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public String Path { get; set; }
        public Int64 Size { get; set; }
        public DateTime DateAdded { get; set; }
        public String SceneName { get; set; }
        public QualityModel Quality { get; set; }
    }
}
