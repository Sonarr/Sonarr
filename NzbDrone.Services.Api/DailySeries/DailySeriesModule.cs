using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using NzbDrone.Services.Api.Extensions;

namespace NzbDrone.Services.Api.DailySeries
{
    public class DailySeriesModule : NancyModule
    {
        private readonly DailySeriesRepository _dailySeriesRepository;

        public DailySeriesModule(DailySeriesRepository dailySeriesRepository)
            : base("/dailyseries")
        {
            _dailySeriesRepository = dailySeriesRepository;

            Get["/"] = x => OnGet();
            Get["/all"] = x => OnGet();
            Get["/{Id}"] = x => OnGet((int)x.Id);
            Get["/isdaily/{Id}"] = x => OnGet((int)x.Id);
        }

        private Response OnGet()
        {
            return _dailySeriesRepository.Public().AsResponse();
        }

        private Response OnGet(int seriesId)
        {
            var result = _dailySeriesRepository.Find(seriesId) != null;

            return result.AsResponse();
        }
    }
}