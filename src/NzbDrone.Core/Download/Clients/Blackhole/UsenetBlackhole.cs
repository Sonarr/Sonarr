using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public class UsenetBlackhole : UsenetClientBase<UsenetBlackholeSettings>
    {
        private readonly IScanWatchFolder _scanWatchFolder;

        public TimeSpan ScanGracePeriod { get; set; }

        public UsenetBlackhole(IScanWatchFolder scanWatchFolder,
                               IHttpClient httpClient,
                               IConfigService configService,
                               IDiskProvider diskProvider,
                               IRemotePathMappingService remotePathMappingService,
                               IValidateNzbs nzbValidationService,
                               Logger logger,
                               ILocalizationService localizationService)
            : base(httpClient, configService, diskProvider, remotePathMappingService, nzbValidationService, logger, localizationService)
        {
            _scanWatchFolder = scanWatchFolder;

            ScanGracePeriod = TimeSpan.FromSeconds(30);
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

        public override string Name => _localizationService.GetLocalizedString("UsenetBlackhole");

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var item in _scanWatchFolder.GetItems(Settings.WatchFolder, ScanGracePeriod))
            {
                yield return new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = Definition.Name + "_" + item.DownloadId,
                    Category = "sonarr",
                    Title = item.Title,

                    TotalSize = item.TotalSize,
                    RemainingTime = item.RemainingTime,

                    OutputPath = item.OutputPath,

                    Status = item.Status,

                    CanBeRemoved = true,
                    CanMoveFiles = true
                };
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (!deleteData)
            {
                throw new NotSupportedException("Blackhole cannot remove DownloadItem without deleting the data as well, ignoring.");
            }

            DeleteItemData(item);
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
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
