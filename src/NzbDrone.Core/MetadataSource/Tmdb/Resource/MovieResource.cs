using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class MovieResource
    {
        public bool adult { get; set; }
        public List<int> genre_ids { get; set; }
        public List<GenreResource> genres { get; set; }
        public int id { get; set; }
        public string imdb_id { get; set; }
        public int runtime { get; set; }
        public string homepage { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string release_date { get; set; }
        public string backdrop_path { get; set; }
        public string poster_path { get; set; }
        public decimal popularity { get; set; }
        public string title { get; set; }
        public string vote_average { get; set; }
        public string overview { get; set; }
        public string tagline { get; set; }
    }

    public class GenreResource
    {
        public int id;
        public string name;
    }
}
