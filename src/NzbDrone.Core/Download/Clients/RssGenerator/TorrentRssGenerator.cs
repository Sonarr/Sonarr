using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients.RssGenerator {

    public class TorrentRssGenerator : DownloadClientBase<TorrentRssGeneratorSettings> {

        private readonly ITorrentRssGeneratorProxy _torrentRssGeneratorProxy;

        public TorrentRssGenerator(ITorrentRssGeneratorProxy torrentRssGeneratorProxy,
                                IConfigService configService,
                                IDiskProvider diskProvider,
                                IRemotePathMappingService remotePathMappingService,
                                Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger) {

            if (torrentRssGeneratorProxy == null)
                throw new ArgumentNullException("torrentRssGeneratorProxy");

            this._torrentRssGeneratorProxy = torrentRssGeneratorProxy;
        }

        public override string Download(RemoteEpisode remoteEpisode) {
            var cached = TorrentRssGeneratorCachedEpisode.CreateFrom(remoteEpisode, DownloadItemStatus.Queued);
            return this._torrentRssGeneratorProxy.AddTorrent(cached, this.Settings);
        }


        public override string Name
        {
            get
            {
                return "Torrent Rss Generator";
            }
        }
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Torrent;
            }
        }

        public override ProviderMessage Message
        {
            get
            {
                return new ProviderMessage("Generates a RSS feed of grabbed downloads that can be accesed via: /feed/torrentrss/rss20.", ProviderMessageType.Info);
            }
        }
        

        public override IEnumerable<DownloadClientItem> GetItems() {
            foreach (var cached in this._torrentRssGeneratorProxy.GetTorrents(this.Settings)) {
                yield return new DownloadClientItem {
                    DownloadClient = this.Definition.Name,
                    DownloadId = cached.Guid,
                    Category = "sonarr-rss",
                    Title = cached.Title,

                    Status = cached.Status,
                    OutputPath = new OsPath(cached.LastKnownLocation),

                    RemainingSize = cached.Size ?? 0,
                    TotalSize = cached.Size ?? 0,

                    IsReadOnly = this.Settings.ReadOnly
                };
            }

        }

        public override void RemoveItem(string downloadId, bool deleteData) {
            if(deleteData) 
                this._logger.Debug("TorrentRssGenerator does not support removal of the torrent data... yet");

            this._torrentRssGeneratorProxy.RemoveTorrent(downloadId, this.Settings);

        }

        public override DownloadClientStatus GetStatus() {
            return new DownloadClientStatus {
                IsLocalhost = true,
                OutputRootFolders = new List<OsPath> { new OsPath(this.Settings.WatchFolder) }
            };
        }

        protected override void Test(List<ValidationFailure> failures) {
            if(this.Settings.IncompleteFolder.IsNotNullOrWhiteSpace())
                failures.AddIfNotNull(this.TestFolder(this.Settings.IncompleteFolder, "IncompleteFolder"));

            if (this.Settings.WatchFolder.IsNotNullOrWhiteSpace())
                failures.AddIfNotNull(this.TestFolder(this.Settings.WatchFolder, "WatchFolder"));
        }
    }
}
