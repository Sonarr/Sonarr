using System;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class EpisodeModel
    {
        public string Title { get; set; }
        public int EpisodeId { get; set; }
        public int EpisodeFileId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Overview { get; set; }
        public string Path { get; set; }
        public String Status { get; set; }
        public string AirDate { get; set; }
        public String Quality { get; set; }
        public int QualityId { get; set; }
        public bool Ignored { get; set; }
    }
}