using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Seasons
{
    public class SeasonModule : NzbDroneRestModule<SeasonResource>
    {
        private readonly ISeasonService _seasonService;

        public SeasonModule(ISeasonService seasonService)
            : base("/season")
        {
            _seasonService = seasonService;

            GetResourceAll = GetSeasons;
            UpdateResource = SetMonitored;
        }

        private List<SeasonResource> GetSeasons()
        {
            var seriesId = Request.Query.SeriesId;

            return ToListResource<Season>(() => _seasonService.GetSeasonsBySeries(seriesId));
        }

        private SeasonResource SetMonitored(SeasonResource seasonResource)
        {
            _seasonService.SetMonitored(seasonResource.SeriesId, seasonResource.SeasonNumber, seasonResource.Monitored);

            return seasonResource;
        }
    }
}