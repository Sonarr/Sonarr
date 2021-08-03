using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class Sabnzbd : UsenetClientBase<SabnzbdSettings>
    {
        private readonly ISabnzbdProxy _proxy;

        public Sabnzbd(ISabnzbdProxy proxy,
                       IHttpClient httpClient,
                       IConfigService configService,
                       IDiskProvider diskProvider,
                       IRemotePathMappingService remotePathMappingService,
                       IValidateNzbs nzbValidationService,
                       Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, nzbValidationService, logger)
        {
            _proxy = proxy;
        }

        // patch can be a number (releases) or 'x' (git)
        private static readonly Regex VersionRegex = new Regex(@"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+|x)", RegexOptions.Compiled);

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var category = Settings.TvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            var response = _proxy.DownloadNzb(fileContent, filename, category, priority, Settings);

            if (response == null || response.Ids.Empty())
            {
                throw new DownloadClientRejectedReleaseException(remoteEpisode.Release, "SABnzbd rejected the NZB for an unknown reason");
            }

            return response.Ids.First();
        }

        private IEnumerable<DownloadClientItem> GetQueue()
        {
            var sabQueue = _proxy.GetQueue(0, 0, Settings);
            var queueItems = new List<DownloadClientItem>();

            foreach (var sabQueueItem in sabQueue.Items)
            {
                if (sabQueueItem.Status == SabnzbdDownloadStatus.Deleted)
                {
                    continue;
                }

                var queueItem = new DownloadClientItem();
                queueItem.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this);
                queueItem.DownloadId = sabQueueItem.Id;
                queueItem.Category = sabQueueItem.Category;
                queueItem.Title = sabQueueItem.Title;
                queueItem.TotalSize = (long)(sabQueueItem.Size * 1024 * 1024);
                queueItem.RemainingSize = (long)(sabQueueItem.Sizeleft * 1024 * 1024);
                queueItem.RemainingTime = sabQueueItem.Timeleft;
                queueItem.CanBeRemoved = true;
                queueItem.CanMoveFiles = true;

                if ((sabQueue.Paused && sabQueueItem.Priority != SabnzbdPriority.Force) ||
                    sabQueueItem.Status == SabnzbdDownloadStatus.Paused)
                {
                    queueItem.Status = DownloadItemStatus.Paused;

                    queueItem.RemainingTime = null;
                }
                else if (sabQueueItem.Status == SabnzbdDownloadStatus.Queued ||
                         sabQueueItem.Status == SabnzbdDownloadStatus.Grabbing ||
                         sabQueueItem.Status == SabnzbdDownloadStatus.Propagating)
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
            var sabHistory = _proxy.GetHistory(0, _configService.DownloadClientHistoryLimit, Settings.TvCategory, Settings);

            var historyItems = new List<DownloadClientItem>();

            foreach (var sabHistoryItem in sabHistory.Items)
            {
                if (sabHistoryItem.Status == SabnzbdDownloadStatus.Deleted)
                {
                    continue;
                }

                var historyItem = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this),
                    DownloadId = sabHistoryItem.Id,
                    Category = sabHistoryItem.Category,
                    Title = sabHistoryItem.Title,

                    TotalSize = sabHistoryItem.Size,
                    RemainingSize = 0,
                    RemainingTime = TimeSpan.Zero,

                    Message = sabHistoryItem.FailMessage,

                    CanBeRemoved = true,
                    CanMoveFiles = true
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
                else
                {
                    // Verifying/Moving etc
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

        public override string Name => "SABnzbd";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var downloadClientItem in GetQueue().Concat(GetHistory()))
            {
                if (downloadClientItem.Category == Settings.TvCategory || (downloadClientItem.Category == "*" && Settings.TvCategory.IsNullOrWhiteSpace()))
                {
                    yield return downloadClientItem;
                }
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            var queueClientItem = GetQueue().SingleOrDefault(v => v.DownloadId == item.DownloadId);

            if (queueClientItem == null)
            {
                if (deleteData && item.Status == DownloadItemStatus.Completed)
                {
                    DeleteItemData(item);
                }

                _proxy.RemoveFrom("history", item.DownloadId, deleteData, Settings);
            }
            else
            {
                _proxy.RemoveFrom("queue", item.DownloadId, deleteData, Settings);
            }
        }

        protected IEnumerable<SabnzbdCategory> GetCategories(SabnzbdConfig config)
        {
            var completeDir = new OsPath(config.Misc.complete_dir);

            if (!completeDir.IsRooted)
            {
                if (HasVersion(2, 0))
                {
                    var status = _proxy.GetFullStatus(Settings);
                    completeDir = new OsPath(status.CompleteDir);
                }
                else
                {
                    var queue = _proxy.GetQueue(0, 1, Settings);
                    var defaultRootFolder = new OsPath(queue.DefaultRootFolder);

                    completeDir = defaultRootFolder + completeDir;
                }
            }

            foreach (var category in config.Categories)
            {
                var relativeDir = new OsPath(category.Dir.TrimEnd('*'));

                category.FullPath = completeDir + relativeDir;

                yield return category;
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var categories = GetCategories(config).ToArray();

            var category = categories.FirstOrDefault(v => v.Name == Settings.TvCategory);

            if (category == null)
            {
                category = categories.FirstOrDefault(v => v.Name == "*");
            }

            var status = new DownloadClientInfo
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
            failures.AddIfNotNull(TestConnectionAndVersion());
            failures.AddIfNotNull(TestAuthentication());
            failures.AddIfNotNull(TestGlobalConfig());
            failures.AddIfNotNull(TestCategory());
        }

        private bool HasVersion(int major, int minor, int patch = 0)
        {
            var rawVersion = _proxy.GetVersion(Settings);
            var version = ParseVersion(rawVersion);

            if (version == null)
            {
                return false;
            }

            if (version.Major > major)
            {
                return true;
            }
            else if (version.Major < major)
            {
                return false;
            }

            if (version.Minor > minor)
            {
                return true;
            }
            else if (version.Minor < minor)
            {
                return false;
            }

            if (version.Build > patch)
            {
                return true;
            }
            else if (version.Build < patch)
            {
                return false;
            }

            return true;
        }

        private Version ParseVersion(string version)
        {
            if (version.IsNullOrWhiteSpace())
            {
                return null;
            }

            var parsed = VersionRegex.Match(version);

            int major;
            int minor;
            int patch;

            if (parsed.Success)
            {
                major = Convert.ToInt32(parsed.Groups["major"].Value);
                minor = Convert.ToInt32(parsed.Groups["minor"].Value);
                patch = Convert.ToInt32(parsed.Groups["patch"].Value.Replace("x", "0"));
            }
            else
            {
                if (!version.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                major = 3;
                minor = 0;
                patch = 0;
            }

            return new Version(major, minor, patch);
        }

        private ValidationFailure TestConnectionAndVersion()
        {
            try
            {
                var rawVersion = _proxy.GetVersion(Settings);
                var version = ParseVersion(rawVersion);

                if (version == null)
                {
                    return new ValidationFailure("Version", "Unknown Version: " + rawVersion);
                }

                if (rawVersion.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new NzbDroneValidationFailure("Version", "Sabnzbd develop version, assuming version 3.0.0 or higher.")
                    {
                        IsWarning = true,
                        DetailedDescription = "Sonarr may not be able to support new features added to SABnzbd when running develop versions."
                    };
                }

                if (version.Major >= 1)
                {
                    return null;
                }

                if (version.Minor >= 7)
                {
                    return null;
                }

                return new ValidationFailure("Version", "Version 0.7.0+ is required, but found: " + version);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("Host", "Unable to connect to SABnzbd")
                       {
                           DetailedDescription = ex.Message
                       };
            }
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
            if (config.Misc.pre_check && !HasVersion(1, 1))
            {
                return new NzbDroneValidationFailure("", "Disable 'Check before download' option in Sabnbzd")
                {
                    InfoLink = _proxy.GetBaseUrl(Settings, "config/switches/"),
                    DetailedDescription = "Using Check before download affects Sonarr ability to track new downloads. Also Sabnzbd recommends 'Abort jobs that cannot be completed' instead since it's more effective."
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
                        InfoLink = _proxy.GetBaseUrl(Settings, "config/categories/"),
                        DetailedDescription = "Sonarr prefers each download to have a separate folder. With * appended to the Folder/Path Sabnzbd will not create these job folders. Go to Sabnzbd to fix it."
                    };
                }
            }
            else
            {
                if (!Settings.TvCategory.IsNullOrWhiteSpace())
                {
                    return new NzbDroneValidationFailure("TvCategory", "Category does not exist")
                    {
                        InfoLink = _proxy.GetBaseUrl(Settings, "config/categories/"),
                        DetailedDescription = "The Category your entered doesn't exist in Sabnzbd. Go to Sabnzbd to create it."
                    };
                }
            }

            if (config.Misc.enable_tv_sorting && ContainsCategory(config.Misc.tv_categories, Settings.TvCategory))
            {
                return new NzbDroneValidationFailure("TvCategory", "Disable TV Sorting")
                {
                    InfoLink = _proxy.GetBaseUrl(Settings, "config/sorting/"),
                    DetailedDescription = "You must disable Sabnzbd TV Sorting for the category Sonarr uses to prevent import issues. Go to Sabnzbd to fix it."
                };
            }

            if (config.Misc.enable_movie_sorting && ContainsCategory(config.Misc.movie_categories, Settings.TvCategory))
            {
                return new NzbDroneValidationFailure("TvCategory", "Disable Movie Sorting")
                {
                    InfoLink = _proxy.GetBaseUrl(Settings, "config/sorting/"),
                    DetailedDescription = "You must disable Sabnzbd Movie Sorting for the category Sonarr uses to prevent import issues. Go to Sabnzbd to fix it."
                };
            }

            if (config.Misc.enable_date_sorting && ContainsCategory(config.Misc.date_categories, Settings.TvCategory))
            {
                return new NzbDroneValidationFailure("TvCategory", "Disable Date Sorting")
                {
                    InfoLink = _proxy.GetBaseUrl(Settings, "config/sorting/"),
                    DetailedDescription = "You must disable Sabnzbd Date Sorting for the category Sonarr uses to prevent import issues. Go to Sabnzbd to fix it."
                };
            }

            return null;
        }

        private bool ContainsCategory(IEnumerable<string> categories, string category)
        {
            if (categories == null || categories.Empty())
            {
                return true;
            }

            if (category.IsNullOrWhiteSpace())
            {
                category = "Default";
            }

            return categories.Contains(category);
        }

        private bool ValidatePath(DownloadClientItem downloadClientItem)
        {
            var downloadItemOutputPath = downloadClientItem.OutputPath;

            if (downloadItemOutputPath.IsEmpty)
            {
                return false;
            }

            if ((OsInfo.IsWindows && !downloadItemOutputPath.IsWindowsPath) ||
                (OsInfo.IsNotWindows && !downloadItemOutputPath.IsUnixPath))
            {
                return false;
            }

            return true;
        }
    }
}
