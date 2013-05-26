using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Series
{
    public class SeriesLookupModule : NzbDroneRestModule<SeriesResource>
    {
        private readonly ISearchForNewSeries _searchProxy;

        public SeriesLookupModule(ISearchForNewSeries searchProxy)
            : base("/Series/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            var tvDbResults = _searchProxy.SearchForNewSeries((string)Request.Query.term);
            return MapToResource(tvDbResults).FirstOrDefault().AsResponse();
        }


        private static IEnumerable<SeriesResource> MapToResource(IEnumerable<Core.Tv.Series> series)
        {
            foreach (var currentSeries in series)
            {
                var resource = currentSeries.InjectTo<SeriesResource>();
                var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}