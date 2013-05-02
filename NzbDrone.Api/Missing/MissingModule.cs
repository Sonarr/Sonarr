using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Missing
{
    public class MissingModule : NzbDroneApiModule
    {
        private readonly IEpisodeService _episodeService;

        public MissingModule(IEpisodeService episodeService)
            : base("/missing")
        {
            _episodeService = episodeService;
            Get["/"] = x => GetMissingEpisodes();
        }

        private Response GetMissingEpisodes()
        {
            bool includeSpecials;
            Boolean.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.IncludeSpecials), out includeSpecials);

            int pageSize;
            Int32.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.PageSize), out pageSize);
            if (pageSize == 0) pageSize = 20;

            int page;
            Int32.TryParse(PrimitiveExtensions.ToNullSafeString(Request.Query.Page), out page);
            if (page == 0) page = 1;

            var sortKey = PrimitiveExtensions.ToNullSafeString(Request.Query.SortKey)
                                                   .Equals("SeriesTitle", StringComparison.InvariantCultureIgnoreCase)
                                                   ? "SeriesTitle"
                                                   : "AirDate";

            var sortDirection = PrimitiveExtensions.ToNullSafeString(Request.Query.SortDir)
                                                   .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                   ? ListSortDirection.Ascending
                                                   : ListSortDirection.Descending;

            var pagingSpec = new PagingSpec<Episode>
                                 {
                                     Page = page,
                                     PageSize = pageSize,
                                     SortKey = sortKey,
                                     SortDirection = sortDirection
                                 };

            var result = _episodeService.EpisodesWithoutFiles(pagingSpec, includeSpecials);
            
            return Mapper.Map<PagingSpec<Episode>, PagingResource<EpisodeResource>>(result).AsResponse();
        }
    }
}