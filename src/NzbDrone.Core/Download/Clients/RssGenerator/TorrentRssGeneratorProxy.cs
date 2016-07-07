using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Disk.Abstractions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.RssGenerator {

    
    public interface ITorrentRssGeneratorProxy {
        IEnumerable<TorrentRssGeneratorCachedEpisode> GetTorrents(TorrentRssGeneratorSettings settings);
        string AddTorrent(TorrentRssGeneratorCachedEpisode episode, TorrentRssGeneratorSettings settings);
        void RemoveTorrent(string id, TorrentRssGeneratorSettings settings);
    }

    public class TorrentRssGeneratorProxy : ITorrentRssGeneratorProxy {
        private readonly ITorrentRssGeneratorStorage _torrentRssGeneratorStorage;
        private readonly ITorrentRssGeneratorCachedEpisodeMatcher _torrentRssGeneratorCachedEpisodeMatcher;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        
        public TorrentRssGeneratorProxy(ITorrentRssGeneratorStorage torrentRssGeneratorStorage,
                                        ITorrentRssGeneratorCachedEpisodeMatcher torrentRssGeneratorCachedEpisodeMatcher,
                                        IDiskProvider diskProvider, 
                                        Logger logger) {
            if (torrentRssGeneratorStorage == null)
                throw new ArgumentNullException("torrentRssGeneratorStorage");
            if (torrentRssGeneratorCachedEpisodeMatcher == null)
                throw new ArgumentNullException("torrentRssGeneratorCachedEpisodeMatcher");
            if (diskProvider == null)
                throw new ArgumentNullException("diskProvider");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this._torrentRssGeneratorStorage = torrentRssGeneratorStorage;
            this._torrentRssGeneratorCachedEpisodeMatcher = torrentRssGeneratorCachedEpisodeMatcher;
            this._diskProvider = diskProvider;
            this._logger = logger;
        }

        public string AddTorrent(TorrentRssGeneratorCachedEpisode episode, TorrentRssGeneratorSettings settings) {
            if(episode == null)
                return null;

            try {
                this._torrentRssGeneratorStorage.Write(episode);
                return episode.Guid;
            }
            catch (Exception ex) {
                throw new DownloadClientException("could not add torrent: [{0}].", ex, episode.Title);
            }
        }


        public IEnumerable<TorrentRssGeneratorCachedEpisode> GetTorrents(TorrentRssGeneratorSettings settings) {
            if(settings == null)
                throw new ArgumentNullException("settings");

            var inprogress = settings.IncompleteFolder.IsNotNullOrWhiteSpace() ?
                                this._diskProvider.GetDirectoryInfo(settings.IncompleteFolder) :
                                null;
            
            // fsi cache for this run
            var inprogressfsis = new Lazy<IFileSystemInfo[]>(() => inprogress.GetFileSystemInfos());

            var completed = settings.WatchFolder.IsNotNullOrWhiteSpace() ?
                                this._diskProvider.GetDirectoryInfo(settings.WatchFolder) :
                                null;

            // fsi cache for this run
            var completedfsis = new Lazy<IFileSystemInfo[]>(() => completed.GetFileSystemInfos());

            foreach (var cached in this._torrentRssGeneratorStorage.All()) {
                if (cached.Status == DownloadItemStatus.Completed) {
                    yield return cached;
                    continue;
                }

                if (completed != null) {
                    var found = this.Find(cached, completedfsis.Value);
                    if (found != null) {
                        cached.Status = DownloadItemStatus.Completed;
                        cached.StatusAt = DateTimeOffset.UtcNow;
                        cached.LastKnownLocation = new OsPath(found.FullName).FullPath;

                        this._torrentRssGeneratorStorage.Write(cached);

                        yield return cached;
                        continue;
                    }
                }

                if (cached.Status == DownloadItemStatus.Downloading) {
                    yield return cached;
                    continue;
                }

                if (inprogress != null) {
                    var found = this.Find(cached, inprogressfsis.Value);
                    if (found != null) {
                        cached.Status = DownloadItemStatus.Downloading;
                        cached.StatusAt = DateTimeOffset.UtcNow;
                        cached.LastKnownLocation = new OsPath(found.FullName).FullPath;

                        this._torrentRssGeneratorStorage.Write(cached);

                        yield return cached;
                        continue;
                    }
                }

                yield return cached;
            }
        }

        public void RemoveTorrent(string id, TorrentRssGeneratorSettings settings) {
            if(id.IsNullOrWhiteSpace())
                return;
            try {
                this._torrentRssGeneratorStorage.Delete(id);
            }
            catch (Exception ex) {
                throw new DownloadClientException("could not remove torrent: [{0}].", ex, id);
            }
        }


        private IFileSystemInfo Find(TorrentRssGeneratorCachedEpisode cached, IFileSystemInfo[] fsis) {
            if (cached == null)
                throw new ArgumentNullException("torrentInfo");

            if (fsis == null || fsis.Length <= 0)
                return null;

            var match = fsis
                .Where(x=> this._torrentRssGeneratorCachedEpisodeMatcher.Matches(cached, x))
                    .FirstOrDefault();

            return match;
        }

       
    }
}
