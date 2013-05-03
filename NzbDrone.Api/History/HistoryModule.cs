using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.History
{
    public class HistoryModule : NzbDroneApiModule
    {
        private readonly IHistoryService _historyService;

        public HistoryModule(IHistoryService historyService)
            : base("/history")
        {
            _historyService = historyService;
            Get["/"] = x => GetHistory();
        }

        private Response GetHistory()
        {
            //TODO: common page parsing logic should be done somewhere else

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

            var series = new Core.Tv.Series { Title = "30 Rock", TitleSlug = "30-rock" };
            var episode = new Episode { Title = "Test", SeasonNumber = 1, EpisodeNumber = 5 };

            var result = new PagingSpec<Core.History.History>
                             {
                                 Records = new List<Core.History.History>
                                               {
                                                   new Core.History.History
                                                       {
                                                           Id = 1,
                                                           Date = DateTime.UtcNow.AddHours(-5),
//                                                           Episode = episode,
//                                                           Series = series,
                                                           Indexer = "nzbs.org",
                                                           Quality = new QualityModel(Quality.HDTV720p)
                                                       }
                                               }
                             };

            result.TotalRecords = result.Records.Count;
            
            return Mapper.Map<PagingSpec<Core.History.History>, PagingResource<HistoryResource>>(result).AsResponse();
        }
    }
}