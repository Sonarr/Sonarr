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

            var sortKey = PrimitiveExtensions.ToNullSafeString(Request.Query.SortKey);
            if (String.IsNullOrEmpty(sortKey)) sortKey = "AirDate";

            var sortDirection = PrimitiveExtensions.ToNullSafeString(Request.Query.SortDir)
                                                   .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                   ? SortDirection.Ascending
                                                   : SortDirection.Descending;

            var pagingSpec = new PagingSpec<Core.History.History>
                                 {
                                     Page = page,
                                     PageSize = pageSize,
                                     SortKey = sortKey,
                                     SortDirection = sortDirection
                                 };

            var result = _historyService.Paged(pagingSpec);
            
            return Mapper.Map<PagingSpec<Core.History.History>, PagingResource<HistoryResource>>(result).AsResponse();
        }
    }
}