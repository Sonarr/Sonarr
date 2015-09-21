using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Api.Movies
{
    public class MovieLookupModule : NzbDroneRestModule<MoviesResource>
    {
        private readonly ISearchForNewMovie _searchProxy;

        public MovieLookupModule(ISearchForNewMovie searchProxy):base("/Movies/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
        }

        private Response Search()
        {
            var results = _searchProxy.SearchForNewMovie((string) Request.Query.term);
            return results.Select(MapToResource).AsResponse();
        }

        private static MoviesResource MapToResource(Core.Movies.Movie currentMovie)
        {
            var resource = currentMovie.InjectTo<MoviesResource>();
            var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
            if (poster != null)
            {
                resource.RemotePoster = poster.Url;
            }
            return resource;
        }
    }
}
