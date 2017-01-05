using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class MoveSeriesService : IExecute<MoveSeriesCommand>
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

        public void Execute(MoveSeriesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var source = message.SourcePath;
            var destination = message.DestinationPath;

            if (!message.DestinationRootFolder.IsNullOrWhiteSpace())
            {
                _logger.Debug("Buiding destination path using root folder: {0} and the series title", message.DestinationRootFolder);
                destination = Path.Combine(message.DestinationRootFolder, _filenameBuilder.GetSeriesFolder(series));
            }

            _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", series.Title, source, destination);

            //TODO: Move to transactional disk operations
            try
            {
                _diskTransferService.TransferFolder(source, destination, TransferMode.Move);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move series from '{0}' to '{1}'", source, destination);
                throw;
            }

            _logger.ProgressInfo("{0} moved successfully to {1}", series.Title, series.Path);

            //Update the series path to the new path
            series.Path = destination;
            series = _seriesService.UpdateSeries(series);

            _eventAggregator.PublishEvent(new SeriesMovedEvent(series, source, destination));
        }
    }
}
