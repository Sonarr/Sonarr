using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        string[] GetVideoFiles(string path, bool allDirectories = true);
    }

    public class DiskScanService :
        IDiskScanService,
        IHandle<SeriesUpdatedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedEpisodes _importApprovedEpisodes;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                                IMakeImportDecision importDecisionMaker,
                                IImportApprovedEpisodes importApprovedEpisodes,
                                ICommandExecutor commandExecutor,
                                IConfigService configService,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedEpisodes = importApprovedEpisodes;
            _commandExecutor = commandExecutor;
            _configService = configService;
            _logger = logger;
        }

        private void Scan(Series series)
        {
            _logger.ProgressInfo("Scanning disk for {0}", series.Title);
            _commandExecutor.PublishCommand(new CleanMediaFileDb(series.Id));

            if (!_diskProvider.FolderExists(series.Path))
            {
                if (_configService.CreateEmptySeriesFolders &&
                    _diskProvider.FolderExists(_diskProvider.GetParentFolder(series.Path)))
                {
                    _logger.Debug("Creating missing series folder: {0}", series.Path);
                    _diskProvider.CreateFolder(series.Path);
                }
                else
                {
                    _logger.Debug("Series folder doesn't exist: {0}", series.Path);
                }

                return;
            }

            var mediaFileList = GetVideoFiles(series.Path).ToList();

            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, series, false);
            _importApprovedEpisodes.Import(decisions);
            _logger.Info("Completed scanning disk for {0}", series.Title);
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(c => MediaFileExtensions.Extensions.Contains(Path.GetExtension(c).ToLower())).ToList();

            _logger.Debug("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList.ToArray();
        }

        public void Handle(SeriesUpdatedEvent message)
        {
            Scan(message.Series);
        }
    }
}