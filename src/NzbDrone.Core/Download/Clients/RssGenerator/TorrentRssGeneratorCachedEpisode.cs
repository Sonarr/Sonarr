using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.RssGenerator {

    [DebuggerDisplay("{Title}")]
    public class TorrentRssGeneratorCachedEpisode {
        private static bool IsMagnetUri(string str) {
            if(str.IsNullOrWhiteSpace())
                return false;

            return str.StartsWith("magnet:", StringComparison.InvariantCultureIgnoreCase);
        }

        public static TorrentRssGeneratorCachedEpisode CreateFrom(RemoteEpisode remoteEpisode, DownloadItemStatus status = DownloadItemStatus.Queued) {
            if (remoteEpisode == null)
                throw new ArgumentNullException("remoteEpisode");
            if (remoteEpisode.Release == null)
                throw new InvalidOperationException("the release information for the RemoteEpisode can not be null.");

            var now = DateTimeOffset.UtcNow;

            var release = remoteEpisode.Release;
            var torrentRelease = remoteEpisode.Release as TorrentInfo;

            var guid = release.Guid;
            var downloadUri = release.DownloadUrl;
            var magnetUri = torrentRelease != null ? torrentRelease.MagnetUrl : null;

            if (guid.IsNullOrWhiteSpace())
                guid = System.Guid.NewGuid().ToString("N");

            if (downloadUri.IsNullOrWhiteSpace())
                downloadUri = magnetUri;

            if(magnetUri.IsNullOrWhiteSpace() && IsMagnetUri(downloadUri))
                magnetUri = downloadUri;

            downloadUri = Uri.UnescapeDataString(downloadUri ?? string.Empty);
            magnetUri = Uri.UnescapeDataString(magnetUri ?? string.Empty);


            return new TorrentRssGeneratorCachedEpisode {
                Title = remoteEpisode.Release.Title,
                Guid = guid,
                DownloadUri = downloadUri,
                MagnetUri = magnetUri,

                InfoUri = release.InfoUrl,
                CommentUri = release.CommentUrl,
                IndexerId = release.IndexerId,
                Indexer = release.Indexer,
                PublishDate = new DateTimeOffset(release.PublishDate).ToUniversalTime(),
                Size = release.Size,

                Peers = torrentRelease != null ? torrentRelease.Peers : null,
                Seeds = torrentRelease != null ? torrentRelease.Seeders : null,
                InfoHash = torrentRelease != null ? torrentRelease.InfoHash : null,
                
                TvdbId = release.TvdbId,
                TvRageId = release.TvRageId,

                EpisodeInfo = remoteEpisode.ParsedEpisodeInfo ?? Parser.Parser.ParseTitle(remoteEpisode.Release.Title),
                Status = status,
                StatusAt = now,
                GrabbedAt = now
            };
        }


        public string Guid { get; set; }
        public string Title { get; set; }
        public string DownloadUri { get; set; }
        public string MagnetUri { get; set; }

        public string InfoUri { get; set; }
        public string CommentUri { get; set; }
        public int? IndexerId { get; set; }
        public string Indexer { get; set; }
        public int? TvdbId { get; set; }
        public int? TvRageId { get; set; }
        public DateTimeOffset? PublishDate { get; set; }

        public int? Peers { get; set; }
        public int? Seeds { get; set; }
        public long? Size { get; set; }
        public string InfoHash { get; set; }
        
        // not supported yet 
        public bool? Verified { get; set; }

        public ParsedEpisodeInfo EpisodeInfo { get; set; }

        public DownloadItemStatus Status { get; set; }
        public string LastKnownLocation { get; set; }

        public DateTimeOffset? GrabbedAt { get; set; }
        public DateTimeOffset? StatusAt { get; set; }

        public bool IsDownloadUriMagnet {
            get {
                return IsMagnetUri(this.DownloadUri);
            }
        }

    }
}
