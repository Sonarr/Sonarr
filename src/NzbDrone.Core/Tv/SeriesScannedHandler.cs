using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesScannedHandler : IHandle<SeriesScannedEvent>,
                                        IHandle<SeriesScanSkippedEvent>
    {
        private readonly IEpisodeMonitoredService _episodeMonitoredService;
        private readonly ISeriesService _seriesService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEpisodeAddedService _episodeAddedService;

        private readonly Logger _logger;

        public SeriesScannedHandler(IEpisodeMonitoredService episodeMonitoredService,
                                    ISeriesService seriesService,
                                    IManageCommandQueue commandQueueManager,
                                    IEpisodeAddedService episodeAddedService,
                                    Logger logger)
        {
            _episodeMonitoredService = episodeMonitoredService;
            _seriesService = seriesService;
            _commandQueueManager = commandQueueManager;
            _episodeAddedService = episodeAddedService;
            _logger = logger;
        }

        private void HandleScanEvents(Series series)
        {
            if (series.AddOptions == null)
            {
                _episodeAddedService.SearchForRecentlyAdded(series.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", series.Title);
            _episodeMonitoredService.SetEpisodeMonitoredStatus(series, series.AddOptions);

            // If both options are enabled search for the whole series, which will only include monitored episodes.
            // This way multiple searches for the same season are skipped, though a season that can't be upgraded may be
            // searched, but the logs will be more explicit.

            if (series.AddOptions.SearchForMissingEpisodes && series.AddOptions.SearchForCutoffUnmetEpisodes)
            {
                _commandQueueManager.Push(new SeriesSearchCommand(series.Id));
            }
            else
            {
                if (series.AddOptions.SearchForMissingEpisodes)
                {
                    _commandQueueManager.Push(new MissingEpisodeSearchCommand(series.Id));
                }

                if (series.AddOptions.SearchForCutoffUnmetEpisodes)
                {
                    _commandQueueManager.Push(new CutoffUnmetEpisodeSearchCommand(series.Id));
                }
            }

            series.AddOptions = null;
            _seriesService.RemoveAddOptions(series);
        }

        public void Handle(SeriesScannedEvent message)
        {
            HandleScanEvents(message.Series);
        }

        public void Handle(SeriesScanSkippedEvent message)
        {
            HandleScanEvents(message.Series);
        }
    }
}
