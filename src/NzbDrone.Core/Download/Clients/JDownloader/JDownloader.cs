using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloader : FilehosterClientBase<JDownloaderSettings>
    {
        private readonly IJDownloaderProxy proxy;

        public JDownloader(IJDownloaderProxy proxy, 
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
            this.proxy = proxy;
        }

        public override string Name
        {
            get
            {
                return "JDownloader";
            }
        }

        private string packagePrefix = "[Sonarr] ";

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;
            return proxy.Download(string.Format("{0}{1}", packagePrefix, remoteEpisode), remoteEpisode.Release.DownloadUrl, priority, Settings);
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var result = new List<DownloadClientItem>();
            var packages = proxy.QueryAllDownloadPackages(Settings).data.Where(a => a.name.StartsWith(packagePrefix));
            foreach (var package in packages)
            {
                var queueItem = new DownloadClientItem()
                {
                    DownloadClient = Definition.Name,
                    TotalSize = package.bytesTotal,
                    Title = package.name.Substring(packagePrefix.Length),
                    RemainingSize = package.bytesTotal - package.bytesLoaded,
                    DownloadId = package.uuid.ToString(),
                };
                _logger.Debug(package.status);
                if (!package.running)
                {
                    if (package.status == "Finished")
                    {
                        queueItem.Status = DownloadItemStatus.Completed;
                        queueItem.Category = "TV";
                        queueItem.OutputPath = new OsPath(package.saveTo);
                    }
                    else
                        queueItem.Status = DownloadItemStatus.Queued;
                }
                else
                    queueItem.Status = DownloadItemStatus.Downloading;

                int eta;
                if (int.TryParse(package.eta, out eta)) queueItem.RemainingTime = TimeSpan.FromSeconds(eta);

                result.Add(queueItem);
            }
            return result;
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(downloadId);
            }

            proxy.RemoveDownload(downloadId, deleteData, Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            return status;
        }

        protected override void Test(List<FluentValidation.Results.ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = proxy.GetVersion(Settings);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsIgnoreCase("Authentication failed"))
                {
                    return new ValidationFailure("Username", "Authentication failed");
                }
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure("Host", "Unable to connect to JDownloader");
            }
            return null;
        }


        public override IList<FilehostLinkCheckInfo> CheckLinks(IList<ReleaseInfo> releases)
        {
            var result = proxy.CheckLinks(releases.Select(a=>a.DownloadUrl).ToList(), Settings);
            var elements = result.Select(a =>
                new FilehostLinkCheckInfo()
                {
                    Title = a.name,
                    IsOnline = a.IsOnline,
                    Size = a.bytesTotal,
                    Release = releases.FirstOrDefault(release => 
                        release.DownloadUrl.EndsWith(a.name) ||
                        EndIsSame(a.url, release.DownloadUrl) ||
                        release.DownloadUrl.StartsWith(a.url))
                }).Where(a=>a.Release != null).ToList();

            return elements;
        }
        private bool EndIsSame(string name, string uri)
        {
            var index1 = name.LastIndexOf('/');
            var index2 = uri.LastIndexOf('/');

            if (index1 < 0 || index2 < 0)
                return false;

            var value = name.Substring(index1, name.Length - index1);
            var value2 = uri.Substring(index2, uri.Length - index2);
            return value == value2;
        }
    }
}
