using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.MovieFiles
{
    public class MovieFileResource : RestResource
    {
        public Int32 MovieId { get; set; }
        public String RelativePath { get; set; }
        public String Path { get; set; }
        public Int64 Size { get; set; }
        public DateTime DateAdded { get; set; }
        public String SceneName { get; set; }
        public QualityModel Quality { get; set; }

        public Boolean QualityCutoffNotMet { get; set; }
    }
}
