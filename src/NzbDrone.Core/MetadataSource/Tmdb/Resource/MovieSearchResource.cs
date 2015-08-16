using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class MovieSearchResource
    {
        public int page;
        public List<MovieResource> results;
        public int total_pages;
        public int total_results;
    }
}
