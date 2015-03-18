using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.UsenetBlackhole
{
    public class UsenetBlackhole : UsenetClientBase<UsenetBlackholeSettings>
    {
        private readonly IDiskScanService _diskScanService;

        public UsenetBlackhole(IDiskScanService diskScanService,
                               IHttpClient httpClient,
                               IConfigService configService,
                               IDiskProvider diskProvider,
                               IRemotePathMappingService remotePathMappingService,
                               Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _diskScanService = diskScanService;
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var title = remoteEpisode.Release.Title;

            title = FileNameBuilder.CleanFileName(title);

            var filepath = Path.Combine(Settings.NzbFolder, title + ".nzb");

            using (var stream = _diskProvider.OpenWriteStream(filepath))
            {
                stream.Write(fileContent, 0, fileContent.Length);
            }

            _logger.Debug("NZB Download succeeded, saved to: {0}", filepath);

            return null;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var folder in _diskProvider.GetDirectories(Settings.WatchFolder))
            {
                var title = FileNameBuilder.CleanFileName(Path.GetFileName(folder));

                var files = _diskProvider.GetFiles(folder, SearchOption.AllDirectories);

                var historyItem = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadId = Definition.Name + "_" + Path.GetFileName(folder) + "_" + _diskProvider.FolderGetCreationTime(folder).Ticks,
                    Category = "nzbdrone",
                    Title = title,

                    TotalSize = files.Select(_diskProvider.GetFileSize).Sum(),

                    OutputPath = new OsPath(folder)
                };

                if (files.Any(_diskProvider.IsFileLocked))
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    historyItem.Status = DownloadItemStatus.Completed;

                    historyItem.RemainingTime = TimeSpan.Zero;
                }

                yield return historyItem;
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(Settings.WatchFolder, false))
            {
                var title = FileNameBuilder.CleanFileName(Path.GetFileName(videoFile));

                var historyItem = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadId = Definition.Name + "_" + Path.GetFileName(videoFile) + "_" + _diskProvider.FileGetLastWrite(videoFile).Ticks,
                    Category = "nzbdrone",
                    Title = title,

                    TotalSize = _diskProvider.GetFileSize(videoFile),

                    OutputPath = new OsPath(videoFile)
                };

                if (_diskProvider.IsFileLocked(videoFile))
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    historyItem.Status = DownloadItemStatus.Completed;
                    historyItem.RemainingTime = TimeSpan.Zero;
                }

                yield return historyItem;
            }
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            var downloadItem = GetItems().FirstOrDefault(v => v.DownloadId == downloadId);

            if (downloadItem == null)
            {
                throw new ArgumentException(string.Format("Cannot remove DownloadItem {0} because it was not found.", downloadId));
            }

            if (!deleteData)
            {
                throw new NotSupportedException("Blackhole cannot remove DownloadItem without deleting the data as well, ignoring.");
            }

            var outputPath = downloadItem.OutputPath.FullPath;

            if (_diskProvider.FileExists(outputPath))
            {
                _diskProvider.DeleteFile(outputPath);
            }
            else
            {
                _diskProvider.DeleteFolder(outputPath, true);
            }
        }

        public override DownloadClientStatus GetStatus()
        {
            return new DownloadClientStatus
            {
                IsLocalhost = true,
                OutputRootFolders = new List<OsPath> { new OsPath(Settings.WatchFolder) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestFolder(Settings.NzbFolder, "NzbFolder"));
            failures.AddIfNotNull(TestFolder(Settings.WatchFolder, "WatchFolder"));
        }
    }
}
