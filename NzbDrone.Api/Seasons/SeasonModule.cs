using System.Collections.Generic;
using NzbDrone.Api.Mapping;
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
            GetResourceById = GetSeason;
            UpdateResource = Update;

            Post["/pass"] = x => SetSeasonPass();
        }

        private List<SeasonResource> GetSeasons()
        {
            var seriesId = Request.Query.SeriesId;

            if (seriesId.HasValue)
            {
                return ToListResource<Season>(() => _seasonService.GetSeasonsBySeries(seriesId));
            }

            return ToListResource(() => _seasonService.GetAllSeasons());
        }

        private SeasonResource GetSeason(int id)
        {
            return _seasonService.Get(id).InjectTo<SeasonResource>();
        }

        private void Update(SeasonResource seasonResource)
        {
            _seasonService.SetMonitored(seasonResource.SeriesId, seasonResource.SeasonNumber, seasonResource.Monitored);
        }

        private List<SeasonResource> SetSeasonPass()
        {
            var seriesId = Request.Form.SeriesId;
            var seasonNumber = Request.Form.SeasonNumber;

            return ToListResource<Season>(() => _seasonService.SetSeasonPass(seriesId, seasonNumber));
        }
    }
}