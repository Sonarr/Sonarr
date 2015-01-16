using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedEpisodesCommandService : IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedEpisodesCommandService(IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                                IDiskProvider diskProvider,
                                                IConfigService configService,
                                                Logger logger)
        {
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        private List<ImportResult> ProcessDroneFactoryFolder()
        {
            var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;

            if (String.IsNullOrEmpty(downloadedEpisodesFolder))
            {
                _logger.Trace("Drone Factory folder is not configured");
                return new List<ImportResult>();
            }

            if (!_diskProvider.FolderExists(downloadedEpisodesFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedEpisodesFolder);
                return new List<ImportResult>();
            }

            return _downloadedEpisodesImportService.ProcessRootFolder(new DirectoryInfo(downloadedEpisodesFolder));
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            var importResults = ProcessDroneFactoryFolder();
            if (importResults == null || importResults.All(v => v.Result != ImportResultType.Imported))
            {
                // Atm we don't report it as a command failure, coz that would cause the download to be failed.
                // Changing the message won't do a thing either, coz it will get set to 'Completed' a msec later.
                //message.SetMessage("Failed to import");
            }
        }
    }
}
