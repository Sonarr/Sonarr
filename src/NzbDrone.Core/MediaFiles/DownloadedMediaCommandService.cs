using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.Commands.Series;
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedMediaCommandService : IExecute<DownloadedEpisodesScanCommand>
    {
        private readonly IDownloadedMediaImportService _downloadedMediaImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedMediaCommandService(IDownloadedMediaImportService downloadedMediaImportService,
                                             ITrackedDownloadService trackedDownloadService,
                                             IDiskProvider diskProvider,
                                             IConfigService configService,
                                             Logger logger)
        {
            _downloadedMediaImportService = downloadedMediaImportService;
            _trackedDownloadService = trackedDownloadService;
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

            return _downloadedMediaImportService.ProcessRootFolder(new DirectoryInfo(downloadedEpisodesFolder));
        }

        private List<ImportResult> ProcessFolder(DownloadedEpisodesScanCommand message)
        {
            if (!_diskProvider.FolderExists(message.Path))
            {
                _logger.Warn("Folder specified for import scan [{0}] doesn't exist.", message.Path);
                return new List<ImportResult>();
            }

            if (message.DownloadClientId.IsNotNullOrWhiteSpace())
            {

                var trackedDownload = _trackedDownloadService.Find(message.DownloadClientId);

                if (trackedDownload != null)
                {
                    _logger.Debug("External directory scan request for known download {0}. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedMediaImportService.ProcessPath(message.Path, trackedDownload.RemoteItem.Media, trackedDownload.DownloadItem);
                }
                else
                {
                    _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedMediaImportService.ProcessPath(message.Path);
                }
            }

            return _downloadedMediaImportService.ProcessPath(message.Path);
        }

        public void Execute(DownloadedEpisodesScanCommand message)
        {
            List<ImportResult> importResults;

            if (message.Path.IsNotNullOrWhiteSpace())
            {
                importResults = ProcessFolder(message);
            }
            else
            {
                importResults = ProcessDroneFactoryFolder();
            }

            if (importResults == null || importResults.All(v => v.Result != ImportResultType.Imported))
            {
                // Atm we don't report it as a command failure, coz that would cause the download to be failed.
                // Changing the message won't do a thing either, coz it will get set to 'Completed' a msec later.
                //message.SetMessage("Failed to import");
            }
        }
    }
}
