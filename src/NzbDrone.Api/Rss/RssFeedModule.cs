using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.RssGenerator;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Api.Rss {

    public class TorrentGeneratorRssFeedModule : NzbDroneFeedModule {

        private class Rss20Formatter {
            private const string DateTimeFormat = "ddd, dd MMM yyyy HH:mm:ss";
            private XNamespace nsSonarr = "https://github.com/Sonarr/Sonarr";

            private static readonly Rss20Formatter __instance = new Rss20Formatter();
            public static Rss20Formatter Instance { get { return __instance; } }

            public string ContentType { get { return "application/xml"; } }
            public Encoding Encoding { get { return Encoding.UTF8; } }

            private Rss20Formatter() {
            }

            public string Serialize(IEnumerable<TorrentRssGeneratorCachedEpisode> cachedEpisodes) {
                XElement channel = null;
                var document =
                 new XDocument(
                     new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "sonarr", nsSonarr),
                        channel = new XElement("channel",
                            new XElement("title", "Sonarr Torrent RSS Feed"),
                            new XElement("description", "Sonarr Torrent RSS Feed")
                        )
                    )
                );

                foreach (var cachedEpisode in cachedEpisodes)
                    channel.Add(this.ToXElement(cachedEpisode));

                return document.ToString(SaveOptions.OmitDuplicateNamespaces);
            }

            private XElement ToXElement(TorrentRssGeneratorCachedEpisode cachedEpisode) {
                var item = new XElement("item",
                   new XElement("title", cachedEpisode.Title),
                   new XElement("category", "tv"),
                   new XElement("guid", cachedEpisode.Guid),
                   new XElement("link", cachedEpisode.DownloadUri),
                   new XElement("pubDate", (cachedEpisode.PublishDate ?? DateTimeOffset.UtcNow).ToUniversalTime().ToString(DateTimeFormat))
                );

                this.AddTorrentInfo(item, cachedEpisode);
                this.AddMagnetInfo(item, cachedEpisode);
                this.AddEpisodeInfo(item, cachedEpisode);
                this.AddEnclosure(item, cachedEpisode);

                return item;
            }

            private void AddTorrentInfo(XElement xe, TorrentRssGeneratorCachedEpisode cachedEpisode) {
                if (xe == null || cachedEpisode == null)
                    return;

                xe.Add(new XElement(nsSonarr + "indexer", cachedEpisode.Indexer));
                xe.Add(new XElement(nsSonarr + "peers", cachedEpisode.Peers));
                xe.Add(new XElement(nsSonarr + "seeds", cachedEpisode.Seeds));
            }
            private void AddMagnetInfo(XElement xe, TorrentRssGeneratorCachedEpisode cachedEpisode) {
                if (xe == null || cachedEpisode == null)
                    return;

                xe.Add(new XElement(nsSonarr + "info_hash", cachedEpisode.InfoHash));
                xe.Add(new XElement(nsSonarr + "magnet_uri", cachedEpisode.MagnetUri));
            }
            private void AddEpisodeInfo(XElement xe, TorrentRssGeneratorCachedEpisode cachedEpisode) {
                if (xe == null || cachedEpisode == null)
                    return;

                var episodeInfo = cachedEpisode.EpisodeInfo;
                if (episodeInfo == null)
                    return;

                var quality = episodeInfo.Quality != null ?
                                (episodeInfo.Quality.Quality != null ? episodeInfo.Quality.Quality.Name : "unknown") :
                                "unknown";

                xe.Add(new XElement(nsSonarr + "parsed_title", episodeInfo.ToString()));
                xe.Add(new XElement(nsSonarr + "series_title", episodeInfo.SeriesTitle));

                xe.Add(new XElement(nsSonarr + "quality", quality));
                xe.Add(new XElement(nsSonarr + "release_hash", episodeInfo.ReleaseHash));
                xe.Add(new XElement(nsSonarr + "release_group", episodeInfo.ReleaseGroup));
                xe.Add(new XElement(nsSonarr + "tvdb_id", cachedEpisode.TvdbId));
                xe.Add(new XElement(nsSonarr + "tvrage_id", cachedEpisode.TvRageId));

                xe.Add(new XElement(nsSonarr + "download_status", cachedEpisode.Status.ToString().ToLower()));
                xe.Add(new XElement(nsSonarr + "status_at", (cachedEpisode.StatusAt ?? DateTimeOffset.UtcNow).ToUniversalTime().ToString(DateTimeFormat)));
                xe.Add(new XElement(nsSonarr + "grabbed_at", (cachedEpisode.GrabbedAt ?? DateTimeOffset.UtcNow).ToUniversalTime().ToString(DateTimeFormat)));
            }

            private void AddEnclosure(XElement xe, TorrentRssGeneratorCachedEpisode cachedEpisode) {
                if (xe == null || cachedEpisode == null)
                    return;

                if (cachedEpisode.DownloadUri.IsNullOrWhiteSpace())
                    return;

                xe.Add(new XElement("enclosure",
                            new XAttribute("url", cachedEpisode.DownloadUri),
                            new XAttribute("length", cachedEpisode.IsDownloadUriMagnet ? 0 : cachedEpisode.Size),
                            new XAttribute("type", "application/x-bittorrent"))
                );
            }

        }

        private readonly ITorrentRssGeneratorStorage _torrentRssGeneratorStorage;

        public TorrentGeneratorRssFeedModule(ITorrentRssGeneratorStorage torrentRssGeneratorStorage)
             : base("torrentrss") {
            if(torrentRssGeneratorStorage == null)
                throw new ArgumentNullException("torrentRssGeneratorStorage");

            this._torrentRssGeneratorStorage = torrentRssGeneratorStorage;

            this.Get["/rss20"] = options => this.GetRss20Feed();
        }

        private Response GetRss20Feed() {
            var items = this._torrentRssGeneratorStorage.All()
                .OrderByDescending(x => x.PublishDate)
                .ToArray();

            var formatter = Rss20Formatter.Instance;
            var content = formatter.Serialize(items);

            return new TextResponse(content, formatter.ContentType, formatter.Encoding);
        }


    }
}
