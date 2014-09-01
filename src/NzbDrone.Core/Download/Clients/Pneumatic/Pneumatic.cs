using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class Pneumatic : DownloadClientBase<PneumaticSettings>
    {
        private readonly IHttpProvider _httpProvider;

        public Pneumatic(IHttpProvider httpProvider,
                         IConfigService configService,
                         IDiskProvider diskProvider,
                         IParsingService parsingService,
                         Logger logger)
            : base(configService, diskProvider, parsingService, logger)
        {
            _httpProvider = httpProvider;
        }

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                throw new NotSupportedException("Full season releases are not supported with Pneumatic.");
            }

            title = FileNameBuilder.CleanFileName(title);

            //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
            var nzbFile = Path.Combine(Settings.NzbFolder, title + ".nzb");

            _logger.Debug("Downloading NZB from: {0} to: {1}", url, nzbFile);
            _httpProvider.DownloadFile(url, nzbFile);

            _logger.Debug("NZB Download succeeded, saved to: {0}", nzbFile);

            WriteStrmFile(title, nzbFile);

            return null;
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Settings.NzbFolder);
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var videoFile in _diskProvider.GetFiles(Settings.StrmFolder, SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(videoFile) != ".strm")
                {
                    continue;
                }

                var title = FileNameBuilder.CleanFileName(Path.GetFileName(videoFile));

                var historyItem = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadClientId = Definition.Name + "_" + Path.GetFileName(videoFile) + "_" + _diskProvider.FileGetLastWriteUtc(videoFile).Ticks,
                    Title = title,

                    TotalSize = _diskProvider.GetFileSize(videoFile),

                    OutputPath = videoFile
                };

                if (_diskProvider.IsFileLocked(videoFile))
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    historyItem.Status = DownloadItemStatus.Completed;
                }

                historyItem.RemoteEpisode = GetRemoteEpisode(historyItem.Title);
                if (historyItem.RemoteEpisode == null) continue;

                yield return historyItem;
            }
        }

        public override void RemoveItem(String id)
        {
            throw new NotSupportedException();
        }

        public override String RetryDownload(String id)
        {
            throw new NotSupportedException();
        }

        public override DownloadClientStatus GetStatus()
        {
            var status = new DownloadClientStatus
            {
                IsLocalhost = true
            };

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestWrite(Settings.NzbFolder, "NzbFolder"));
            failures.AddIfNotNull(TestWrite(Settings.StrmFolder, "StrmFolder"));
        }

        private ValidationFailure TestWrite(String folder, String propertyName)
        {
            if (!_diskProvider.FolderExists(folder))
            {
                return new ValidationFailure(propertyName, "Folder does not exist");
            }

            try
            {
                var testPath = Path.Combine(folder, "drone_test.txt");
                _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
                _diskProvider.DeleteFile(testPath);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure(propertyName, "Unable to write to folder");
            }

            return null;
        }

        private void WriteStrmFile(String title, String nzbFile)
        {
            String folder;

            if (Settings.StrmFolder.IsNullOrWhiteSpace())
            {
                folder = _configService.DownloadedEpisodesFolder;

                if (folder.IsNullOrWhiteSpace())
                {
                    throw new DownloadClientException("Strm Folder needs to be set for Pneumatic Downloader");
                }
            }

            else
            {
                folder = Settings.StrmFolder;
            }

            var contents = String.Format("plugin://plugin.program.pneumatic/?mode=strm&type=add_file&nzb={0}&nzbname={1}", nzbFile, title);
            _diskProvider.WriteAllText(Path.Combine(folder, title + ".strm"), contents);
        }
    }
}
