using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class MoveSeriesService : IExecute<MoveSeriesCommand>, IExecute<BulkMoveSeriesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveSeriesService(ISeriesService seriesService,
                                 IBuildFileNames filenameBuilder,
                                 IDiskTransferService diskTransferService,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _seriesService = seriesService;
            _filenameBuilder = filenameBuilder;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void MoveSingleSeries(Series series, string sourcePath, string destinationPath)
        {
            _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", series.Title, sourcePath, destinationPath);

            try
            {
                _diskTransferService.TransferFolder(sourcePath, destinationPath, TransferMode.Move);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move series from '{0}' to '{1}'. Try moving files manually", sourcePath, destinationPath);

                RevertPath(series.Id, sourcePath);
            }

            _logger.ProgressInfo("{0} moved successfully to {1}", series.Title, series.Path);

            _eventAggregator.PublishEvent(new SeriesMovedEvent(series, sourcePath, destinationPath));
        }

        private void RevertPath(int seriesId, string path)
        {
            var series = _seriesService.GetSeries(seriesId);

            series.Path = path;
            _seriesService.UpdateSeries(series);
        }

        public void Execute(MoveSeriesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            MoveSingleSeries(series, message.SourcePath, message.DestinationPath);
        }

        public void Execute(BulkMoveSeriesCommand message)
        {
            var seriesToMove = message.Series;
            var destinationRootFolder = message.DestinationRootFolder;

            _logger.ProgressInfo("Moving {0} series to '{1}'", seriesToMove.Count, destinationRootFolder);

            foreach (var s in seriesToMove)
            {
                var series = _seriesService.GetSeries(s.SeriesId);
                var destinationPath = Path.Combine(destinationRootFolder, _filenameBuilder.GetSeriesFolder(series));

                MoveSingleSeries(series, s.SourcePath, destinationPath);
            }

            _logger.ProgressInfo("Finished moving {0} series to '{1}'", seriesToMove.Count, destinationRootFolder);
        }
    }
}
