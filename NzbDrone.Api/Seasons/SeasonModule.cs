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

            Post["/pass"] = x => SetSeasonPass();
        }

        private List<SeasonResource> GetSeasons()
        {
            var seriesId = Request.Query.SeriesId;

            if (seriesId.HasValue)
            {
                return ToListResource<Season>(() => _seasonService.GetSeasonsBySeries(seriesId));
            }

            return ToListResource<Season>(() => _seasonService.GetAllSeasons());
        }

        private SeasonResource SetMonitored(SeasonResource seasonResource)
        {
            _seasonService.SetMonitored(seasonResource.SeriesId, seasonResource.SeasonNumber, seasonResource.Monitored);

            return seasonResource;
        }

        private List<SeasonResource> SetSeasonPass()
        {
            var seriesId = Request.Form.SeriesId;
            var seasonNumber = Request.Form.SeasonNumber;

            return ToListResource<Season>(() => _seasonService.SetSeasonPass(seriesId, seasonNumber));
        }
    }
}