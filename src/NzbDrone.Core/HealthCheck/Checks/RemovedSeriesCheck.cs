using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(SeriesUpdatedEvent))]
    [CheckOn(typeof(SeriesDeletedEvent))]
    [CheckOn(typeof(SeriesRefreshCompleteEvent))]
    public class RemovedSeriesCheck : HealthCheckBase, ICheckOnCondition<SeriesUpdatedEvent>, ICheckOnCondition<SeriesDeletedEvent>
    {
        private readonly ISeriesService _seriesService;

        public RemovedSeriesCheck(ISeriesService seriesService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _seriesService = seriesService;
        }

        public override HealthCheck Check()
        {
            var deletedSeries = _seriesService.GetAllSeries().Where(v => v.Status == SeriesStatusType.Deleted).ToList();

            if (deletedSeries.Empty())
            {
                return new HealthCheck(GetType());
            }

            var seriesText = deletedSeries.Select(s => $"{s.Title} (tvdbid {s.TvdbId})").Join(", ");

            if (deletedSeries.Count == 1)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("RemovedSeriesSingleRemovedHealthCheckMessage", new Dictionary<string, object>
                    {
                        { "series", seriesText }
                    }),
                    "#series-removed-from-thetvdb");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Error,
                _localizationService.GetLocalizedString("RemovedSeriesMultipleRemovedHealthCheckMessage", new Dictionary<string, object>
                {
                    { "series", seriesText }
                }),
                "#series-removed-from-thetvdb");
        }

        public bool ShouldCheckOnEvent(SeriesDeletedEvent deletedEvent)
        {
            return deletedEvent.Series.Any(s => s.Status == SeriesStatusType.Deleted);
        }

        public bool ShouldCheckOnEvent(SeriesUpdatedEvent updatedEvent)
        {
            return updatedEvent.Series.Status == SeriesStatusType.Deleted;
        }
    }
}
