using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesScannedHandler : IHandleAsync<SeriesScannedEvent>,
                                        IHandleAsync<SeriesScanSkippedEvent>
    {
        private readonly IEpisodeMonitoredService _episodeMonitoredService;
        private readonly ISeriesService _seriesService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeRefreshedService _episodeRefreshedService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Logger _logger;

        public SeriesScannedHandler(IEpisodeMonitoredService episodeMonitoredService,
                                    ISeriesService seriesService,
                                    IManageCommandQueue commandQueueManager,
                                    IEpisodeRefreshedService episodeRefreshedService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _episodeMonitoredService = episodeMonitoredService;
            _seriesService = seriesService;
            _commandQueueManager = commandQueueManager;
            _episodeRefreshedService = episodeRefreshedService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void HandleScanEvents(Series series)
        {
            var addOptions = series.AddOptions;

            if (addOptions == null)
            {
                _episodeRefreshedService.Search(series.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", series.Title);
            _episodeMonitoredService.SetEpisodeMonitoredStatus(series, addOptions);

            _eventAggregator.PublishEventAsync(new SeriesAddCompletedEvent(series)).GetAwaiter().GetResult();

            // If both options are enabled search for the whole series, which will only include monitored episodes.
            // This way multiple searches for the same season are skipped, though a season that can't be upgraded may be
            // searched, but the logs will be more explicit.

            if (addOptions.SearchForMissingEpisodes && addOptions.SearchForCutoffUnmetEpisodes)
            {
                _commandQueueManager.Push(new SeriesSearchCommand(series.Id));
            }
            else
            {
                if (addOptions.SearchForMissingEpisodes)
                {
                    _commandQueueManager.Push(new MissingEpisodeSearchCommand(series.Id));
                }

                if (addOptions.SearchForCutoffUnmetEpisodes)
                {
                    _commandQueueManager.Push(new CutoffUnmetEpisodeSearchCommand(series.Id));
                }
            }

            series.AddOptions = null;
            _seriesService.RemoveAddOptions(series);
        }

        public async Task HandleAsync(SeriesScannedEvent message, CancellationToken cancellationToken)
        {
            // TODO: Make database and command queue operations async
            await Task.Run(() =>
            {
                HandleScanEvents(message.Series);
            },
            cancellationToken);
        }

        public async Task HandleAsync(SeriesScanSkippedEvent message, CancellationToken cancellationToken)
        {
            // TODO: Make database and command queue operations async
            await Task.Run(() =>
            {
                HandleScanEvents(message.Series);
            },
            cancellationToken);
        }
    }
}
