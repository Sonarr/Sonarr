using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using NzbDrone.Services.Api.NancyExtensions;

namespace NzbDrone.Services.Api.DailySeries
{
    public class DailySeriesModule : NancyModule
    {
        private readonly DailySeriesProvider _dailySeriesProvider;

        public DailySeriesModule(DailySeriesProvider dailySeriesProvider)
            : base("/dailyseries")
        {
            _dailySeriesProvider = dailySeriesProvider;

            Get["/"] = x => OnGet();
            Get["/all"] = x => OnGet();
            Get["/{Id}"] = x => OnGet((int)x.Id);
            Get["/isdaily/{Id}"] = x => OnGet((int)x.Id);
        }

        private Response OnGet()
        {
            return _dailySeriesProvider.Public().AsResponse();
        }

        private Response OnGet(int seriesId)
        {
            return _dailySeriesProvider.IsDaily(seriesId).AsResponse();
        }
    }
}