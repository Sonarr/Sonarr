using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DataAugmentation.DailySeries
{
    public class DailySeriesService
    {
        //TODO: add timer command

        private readonly IDailySeriesDataProxy _proxy;
        private readonly ISeriesService _seriesService;

        public DailySeriesService(IDailySeriesDataProxy proxy, ISeriesService seriesService)
        {
            _proxy = proxy;
            _seriesService = seriesService;
        }

        public virtual void UpdateDailySeries()
        {
            var dailySeries = _proxy.GetDailySeriesIds();

            foreach (var tvdbId in dailySeries)
            {
                var series = _seriesService.FindByTvdbId(tvdbId);

                if (series != null)
                {
                    _seriesService.SetSeriesType(series.Id, SeriesTypes.Daily);
                }
            }
        }
    }
}
