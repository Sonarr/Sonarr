using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DataAugmentation.DailySeries
{
    public interface IDailySeriesService
    {
        void UpdateDailySeries();
        bool IsDailySeries(int tvdbid);
    }

    public class DailySeriesService : IDailySeriesService
    {
        //TODO: add timer command

        private readonly IDailySeriesDataProxy _proxy;
        private readonly ISeriesService _seriesService;

        public DailySeriesService(IDailySeriesDataProxy proxy, ISeriesService seriesService)
        {
            _proxy = proxy;
            _seriesService = seriesService;
        }

        public void UpdateDailySeries()
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

        public bool IsDailySeries(int tvdbid)
        {
            return _proxy.IsDailySeries(tvdbid);
        }
    }
}
