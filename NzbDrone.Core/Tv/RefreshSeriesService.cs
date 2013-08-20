using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.DataAugmentation.DailySeries;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Common;

namespace NzbDrone.Core.Tv
{
    public class RefreshSeriesService : IExecute<RefreshSeriesCommand>, IHandleAsync<SeriesAddedEvent>
    {
        private readonly IProvideSeriesInfo _seriesInfo;
        private readonly ISeriesService _seriesService;
        private readonly IRefreshEpisodeService _refreshEpisodeService;
        private readonly IMessageAggregator _messageAggregator;
        private readonly IDailySeriesService _dailySeriesService;
        private readonly Logger _logger;

        public RefreshSeriesService(IProvideSeriesInfo seriesInfo, ISeriesService seriesService, IRefreshEpisodeService refreshEpisodeService, IMessageAggregator messageAggregator, IDailySeriesService dailySeriesService, Logger logger)
        {
            _seriesInfo = seriesInfo;
            _seriesService = seriesService;
            _refreshEpisodeService = refreshEpisodeService;
            _messageAggregator = messageAggregator;
            _dailySeriesService = dailySeriesService;
            _logger = logger;
        }


        public void Execute(RefreshSeriesCommand message)
        {
            if (message.SeriesId.HasValue)
            {
                var series = _seriesService.GetSeries(message.SeriesId.Value);
                RefreshSeriesInfo(series);
            }
            else
            {
                var allSeries = _seriesService.GetAllSeries().OrderBy(c => c.LastInfoSync).ToList();

                foreach (var series in allSeries)
                {
                    try
                    {
                        RefreshSeriesInfo(series);
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Couldn't refresh info for {0}".Inject(series), e);
                    }
                }
            }
        }

        public void HandleAsync(SeriesAddedEvent message)
        {
            RefreshSeriesInfo(message.Series);
        }

        private void RefreshSeriesInfo(Series series)
        {
            var tuple = _seriesInfo.GetSeriesInfo(series.TvdbId);

            var seriesInfo = tuple.Item1;

            series.Title = seriesInfo.Title;
            series.AirTime = seriesInfo.AirTime;
            series.Overview = seriesInfo.Overview;
            series.Status = seriesInfo.Status;
            series.CleanTitle = seriesInfo.CleanTitle;
            series.LastInfoSync = DateTime.UtcNow;
            series.Runtime = seriesInfo.Runtime;
            series.Images = seriesInfo.Images;
            series.Network = seriesInfo.Network;
            series.FirstAired = seriesInfo.FirstAired;

            if (_dailySeriesService.IsDailySeries(series.TvdbId))
            {
                series.SeriesType = SeriesTypes.Daily;
            }

            try
            {
                series.Path = new DirectoryInfo(series.Path).FullName;
                series.Path = series.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.WarnException("Couldn't update series path for " + series.Path, e);
            }


            _seriesService.UpdateSeries(series);

            _refreshEpisodeService.RefreshEpisodeInfo(series, tuple.Item2);

            _messageAggregator.PublishEvent(new SeriesUpdatedEvent(series));
        }


    }
}