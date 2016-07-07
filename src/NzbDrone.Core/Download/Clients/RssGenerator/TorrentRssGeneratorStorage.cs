using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Download.Clients.RssGenerator {
    public interface ITorrentRssGeneratorStorage {
        string Write(TorrentRssGeneratorCachedEpisode TorrentRssGeneratorCachedEpisode);
        TorrentRssGeneratorCachedEpisode Read(string id);
        void Delete(string id);

        IEnumerable<TorrentRssGeneratorCachedEpisode> All();

        TorrentRssGeneratorCachedEpisode this[string id]
        {
            get;
        }
    }

    public class AppFolderTorrentRssGeneratorStorage : ITorrentRssGeneratorStorage {
        public const string RSS_CACHE_FOLDER = "rsscache";

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        private readonly string _rssCacheLocation;

        public AppFolderTorrentRssGeneratorStorage(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger) {
            if (appFolderInfo == null)
                throw new ArgumentNullException("appFolderInfo");
            if (diskProvider == null)
                throw new ArgumentNullException("diskProvider");
            if (logger == null)
                throw new ArgumentNullException("logger");

            this._appFolderInfo = appFolderInfo;
            this._diskProvider = diskProvider;
            this._logger = logger;

            this._rssCacheLocation = Path.Combine(this._appFolderInfo.GetAppDataPath(), RSS_CACHE_FOLDER);
        }

        public TorrentRssGeneratorCachedEpisode this[string id]
        {
            get
            {
                return this.Read(id);
            }
        }

        public IEnumerable<TorrentRssGeneratorCachedEpisode> All() {
            return this.Enumerate()
                    .Where(x => x.Item2 != null)
                        .Select(x => x.Item2);
        }

        public void Delete(string id) {
            var found = this.Enumerate().Where(x => x.Item2 != null && x.Item2.Guid == id).FirstOrDefault();
            if (found != null)
                this._diskProvider.DeleteFile(found.Item1);
        }

        public TorrentRssGeneratorCachedEpisode Read(string id) {
            return this.All().Where(x => x.Guid == id).FirstOrDefault();
        }

        public string Write(TorrentRssGeneratorCachedEpisode torrentRssGeneratorCachedEpisode) {
            if (torrentRssGeneratorCachedEpisode == null)
                throw new ArgumentNullException("TorrentRssGeneratorCachedEpisode");

            var title = FileNameBuilder.CleanFileName(torrentRssGeneratorCachedEpisode.Title);
            this._diskProvider.EnsureFolder(this._rssCacheLocation);

            var filepath = Path.Combine(this._rssCacheLocation, string.Format("{0}.rsscache.json", title));

            using (var stream = this._diskProvider.OpenWriteStream(filepath))
            using (var writer = new StreamWriter(stream)) {
                var json = JsonConvert.SerializeObject(torrentRssGeneratorCachedEpisode, Formatting.Indented);
                writer.Write(json);
            }

            this._logger.Debug("torrent rss cache saved to: [{0}]", filepath);

            return torrentRssGeneratorCachedEpisode.Guid;
        }

        public IEnumerable<Tuple<string, TorrentRssGeneratorCachedEpisode>> Enumerate() {
            if (!this._diskProvider.FolderExists(this._rssCacheLocation))
                yield break;

            foreach (var file in this._diskProvider.GetFiles(this._rssCacheLocation, SearchOption.TopDirectoryOnly)) {
                TorrentRssGeneratorCachedEpisode torrentInfo = null;
                try {
                    var json = this._diskProvider.ReadAllText(file);
                    torrentInfo = JsonConvert.DeserializeObject<TorrentRssGeneratorCachedEpisode>(json);
                    if (torrentInfo == null || torrentInfo.Guid.IsNullOrWhiteSpace() || torrentInfo.Title.IsNullOrWhiteSpace()) {
                        this._logger.Warn("the rss cache file [{0}] does seem not contian valid information.", file);
                        continue;
                    }
                }
                catch (Exception ex) {
                    this._logger.Warn(ex, "the rss cache file [{0}] seems to be invalid.", file);
                    continue;
                }

                yield return Tuple.Create(file, torrentInfo);
            }
        }
    }
}
