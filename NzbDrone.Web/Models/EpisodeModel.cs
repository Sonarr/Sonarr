using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class EpisodeModel
    {
        public string Title { get; set; }
        public int EpisodeId { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Overview { get; set; }
        public string Path { get; set; }
        public DateTime AirDate { get; set; }

        public String Quality { get; set; }
    }
}