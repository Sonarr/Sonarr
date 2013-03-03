using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Seasons
{
    public class SeasonModule : NzbDroneApiModule
    {
        private readonly ISeasonService _seasonService;

        public SeasonModule(ISeasonService seasonService)
            : base("/Season")
        {
            _seasonService = seasonService;

            Get["/"] = x => GetSeasons();
        }

        private Response GetSeasons()
        {
            var seriesId = Request.Query.SeriesId;

            return JsonExtensions.AsResponse(_seasonService.GetSeasonsBySeries(seriesId));
        }
    }


}