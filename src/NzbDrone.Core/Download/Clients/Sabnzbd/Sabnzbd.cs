using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class Sabnzbd : UsenetClientBase<SabnzbdSettings>
    {
        private readonly ISabnzbdProxy _proxy;

        public Sabnzbd(ISabnzbdProxy proxy,
                       IHttpClient httpClient,
                       IConfigService configService,
                       IDiskProvider diskProvider,
                       IParsingService parsingService,
                       IRemotePathMappingService remotePathMappingService,
                       Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var title = remoteEpisode.Release.Title;
            var category = Settings.TvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            var response = _proxy.DownloadNzb(fileContent, title, category, priority, Settings);

            if (response != null && response.Ids.Any())
            {
                return response.Ids.First();
            }

            return null;
        }

        private IEnumerable<DownloadClientItem> GetQueue()
        {
            SabnzbdQueue sabQueue;

            try
            {
                sabQueue = _proxy.GetQueue(0, 0, Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Warn("Couldn't get download queue. {0}", ex.Message);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            foreach (var sabQueueItem in sabQueue.Items)
            {
                var queueItem = new DownloadClientItem();
                queueItem.DownloadClient = Definition.Name;
                queueItem.DownloadClientId = sabQueueItem.Id;
                queueItem.Category = sabQueueItem.Category;
                queueItem.Title = sabQueueItem.Title;
                queueItem.TotalSize = (long)(sabQueueItem.Size * 1024 * 1024);
                queueItem.RemainingSize = (long)(sabQueueItem.Sizeleft * 1024 * 1024);
                queueItem.RemainingTime = sabQueueItem.Timeleft;

                if (sabQueue.Paused || sabQueueItem.Status == SabnzbdDownloadStatus.Paused)
                {
                    queueItem.Status = DownloadItemStatus.Paused;

                    queueItem.RemainingTime = null;
                }
                else if (sabQueueItem.Status == SabnzbdDownloadStatus.Queued || sabQueueItem.Status == SabnzbdDownloadStatus.Grabbing)
                {
                    queueItem.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    queueItem.Status = DownloadItemStatus.Downloading;
                }

                if (queueItem.Title.StartsWith("ENCRYPTED /"))
                {
                    queueItem.Title = queueItem.Title.Substring(11);
                    queueItem.IsEncrypted = true;
                }

                queueItems.Add(queueItem);
            }

            return queueItems;
        }

        private IEnumerable<DownloadClientItem> GetHistory()
        {
            SabnzbdHistory sabHistory;

            try
            {
                sabHistory = _proxy.GetHistory(0, _configService.DownloadClientHistoryLimit, Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var historyItems = new List<DownloadClientItem>();

            foreach (var sabHistoryItem in sabHistory.Items)
            {
                var historyItem = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadClientId = sabHistoryItem.Id,
                    Category = sabHistoryItem.Category,
                    Title = sabHistoryItem.Title,

                    TotalSize = sabHistoryItem.Size,
                    RemainingSize = 0,
                    DownloadTime = TimeSpan.FromSeconds(sabHistoryItem.DownloadTime),
                    RemainingTime = TimeSpan.Zero,

                    Message = sabHistoryItem.FailMessage
                };

                if (sabHistoryItem.Status == SabnzbdDownloadStatus.Failed)
                {
                    if (sabHistoryItem.FailMessage.IsNotNullOrWhiteSpace() &&
                        sabHistoryItem.FailMessage.Equals("Unpacking failed, write error or disk is full?", StringComparison.InvariantCultureIgnoreCase))
                    {
                        historyItem.Status = DownloadItemStatus.Warning;
                    }
                    else
                    {
                        historyItem.Status = DownloadItemStatus.Failed;
                    }
                }
                else if (sabHistoryItem.Status == SabnzbdDownloadStatus.Completed)
                {
                    historyItem.Status = DownloadItemStatus.Completed;
                }
                else // Verifying/Moving etc
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(sabHistoryItem.Storage));

                if (!outputPath.IsEmpty)
                {
                    historyItem.OutputPath = outputPath;

                    var parent = outputPath.Directory;
                    while (!parent.IsEmpty)
                    {
                        if (parent.FileName == sabHistoryItem.Title)
                        {
                            historyItem.OutputPath = parent;
                        }
                        parent = parent.Directory;
                    }
                }

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            MigrateLocalCategoryPath();

            foreach (var downloadClientItem in GetQueue().Concat(GetHistory()))
            {
                if (downloadClientItem.Category == Settings.TvCategory)
                {
                    yield return downloadClientItem;
                }
            }
        }

        public override void RemoveItem(String id)
        {
            if (GetQueue().Any(v => v.DownloadClientId == id))
            {
                _proxy.RemoveFrom("queue", id, Settings);
            }
            else
            {
                _proxy.RemoveFrom("history", id, Settings);
            }
        }

        public override String RetryDownload(String id)
        {
            // Sabnzbd changed the nzo_id for retried downloads without reporting it back to us. We need to try to determine the new ID.
            // Check both the queue and history because sometimes SAB keeps item in history to retry post processing (depends on failure reason)

            var currentHistory = GetHistory().ToList();
            var currentHistoryItems = currentHistory.Where(v => v.DownloadClientId == id).ToList();

            if (currentHistoryItems.Count != 1)
            {
                _logger.Warn("History item missing. Couldn't get the new nzoid.");
                return id;
            }

            var currentHistoryItem = currentHistoryItems.First();
            var otherItemsWithSameTitle = currentHistory.Where(h => h.Title == currentHistoryItem.Title &&
                                                               h.DownloadClientId != currentHistoryItem.DownloadClientId).ToList();

            _proxy.RetryDownload(id, Settings);

            for (int i = 0; i < 3; i++)
            {
                var queue = GetQueue().Where(v => v.Category == currentHistoryItem.Category &&
                                                  v.Title == currentHistoryItem.Title).ToList();

                var history = GetHistory().Where(v => v.Category == currentHistoryItem.Category &&
                                                      v.Title == currentHistoryItem.Title &&
                                                      !otherItemsWithSameTitle.Select(h => h.DownloadClientId)
                                                                             .Contains(v.DownloadClientId)).ToList();

                if (queue.Count == 1)
                {
                    return queue.First().DownloadClientId;
                }

                if (history.Count == 1)
                {
                    return history.First().DownloadClientId;
                }

                if (queue.Count > 1 || history.Count > 1)
                {
                    _logger.Warn("Multiple items with the correct title. Couldn't get the new nzoid.");
                    return id;
                }

                Thread.Sleep(500);
            }

            _logger.Warn("No items with the correct title. Couldn't get the new nzoid.");
            return id;
        }

        protected IEnumerable<SabnzbdCategory> GetCategories(SabnzbdConfig config)
        {
            var completeDir = new OsPath(config.Misc.complete_dir);

            if (!completeDir.IsRooted)
            {
                var queue = _proxy.GetQueue(0, 1, Settings);
                var defaultRootFolder = new OsPath(queue.DefaultRootFolder);

                completeDir = defaultRootFolder + completeDir;
            }

            foreach (var category in config.Categories)
            {
                var relativeDir = new OsPath(category.Dir.TrimEnd('*'));

                category.FullPath = completeDir + relativeDir;

                yield return category;
            }
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var categories = GetCategories(config).ToArray();

            var category = categories.FirstOrDefault(v => v.Name == Settings.TvCategory);

            if (category == null)
            {
                category = categories.FirstOrDefault(v => v.Name == "*");
            }

            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            if (category != null)
            {
                status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, category.FullPath) };
            }

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestAuthentication());
            failures.AddIfNotNull(TestGlobalConfig());
            failures.AddIfNotNull(TestCategory());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetVersion(Settings);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure("Host", "Unable to connect to SABnzbd");
            }

            return null;
        }

        private ValidationFailure TestAuthentication()
        {
            try
            {
                _proxy.GetConfig(Settings);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsIgnoreCase("API Key Incorrect"))
                {
                    return new ValidationFailure("APIKey", "API Key Incorrect");
                }
                if (ex.Message.ContainsIgnoreCase("API Key Required"))
                {
                    return new ValidationFailure("APIKey", "API Key Required");
                }
                throw;
            }

            return null;
        }

        private ValidationFailure TestGlobalConfig()
        {
            var config = _proxy.GetConfig(Settings);
            if (config.Misc.pre_check)
            {
                return new NzbDroneValidationFailure("", "Disable 'Check before download' option in Sabnbzd")
                {
                    InfoLink = String.Format("http://{0}:{1}/sabnzbd/config/switches/", Settings.Host, Settings.Port),
                    DetailedDescription = "Using Check before download affects NzbDrone ability to track new downloads. Also Sabnzbd recommends 'Abort jobs that cannot be completed' instead since it's more effective."
                };
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            var config = _proxy.GetConfig(Settings);
            var category = GetCategories(config).FirstOrDefault((SabnzbdCategory v) => v.Name == Settings.TvCategory);

            if (category != null)
            {
                if (category.Dir.EndsWith("*"))
                {
                    return new NzbDroneValidationFailure("TvCategory", "Enable Job folders")
                    {
                        InfoLink = String.Format("http://{0}:{1}/sabnzbd/config/categories/", Settings.Host, Settings.Port),
                        DetailedDescription = "NzbDrone prefers each download to have a separate folder. With * appended to the Folder/Path Sabnzbd will not create these job folders. Go to Sabnzbd to fix it."
                    };
                }
            }
            else
            {
                if (!Settings.TvCategory.IsNullOrWhiteSpace())
                {
                    return new NzbDroneValidationFailure("TvCategory", "Category does not exist")
                    {
                        InfoLink = String.Format("http://{0}:{1}/sabnzbd/config/categories/", Settings.Host, Settings.Port),
                        DetailedDescription = "The Category your entered doesn't exist in Sabnzbd. Go to Sabnzbd to create it."
                    };
                }
            }

            if (config.Misc.enable_tv_sorting)
            {
                if (!config.Misc.tv_categories.Any<string>() ||
                    config.Misc.tv_categories.Contains(Settings.TvCategory) ||
                    (Settings.TvCategory.IsNullOrWhiteSpace() && config.Misc.tv_categories.Contains("Default")))
                {
                    return new NzbDroneValidationFailure("TvCategory", "Disable TV Sorting")
                    {
                        InfoLink = String.Format("http://{0}:{1}/sabnzbd/config/sorting/", Settings.Host, Settings.Port),
                        DetailedDescription = "You must disable Sabnzbd TV Sorting for the category NzbDrone uses to prevent import issues. Go to Sabnzbd to fix it."
                    };
                }
            }

            return null;
        }

        private void MigrateLocalCategoryPath()
        {
            // TODO: Remove around January 2015, this code moves the settings to the RemotePathMappingService.
            if (!Settings.TvCategoryLocalPath.IsNullOrWhiteSpace())
            {
                try
                {
                    _logger.Debug("Has legacy TvCategoryLocalPath, trying to migrate to RemotePathMapping list.");

                    var config = _proxy.GetConfig(Settings);
                    var category = GetCategories(config).FirstOrDefault(v => v.Name == Settings.TvCategory);

                    if (category != null)
                    {
                        var localPath = new OsPath(Settings.TvCategoryLocalPath);
                        Settings.TvCategoryLocalPath = null;

                        _remotePathMappingService.MigrateLocalCategoryPath(Definition.Id, Settings, Settings.Host, category.FullPath, localPath);

                        _logger.Info("Discovered Local Category Path for {0}, the setting was automatically moved to the Remote Path Mapping table.", Definition.Name);
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.ErrorException("Unable to migrate local category path", ex);
                    throw;
                }
            }
        }
    }
}