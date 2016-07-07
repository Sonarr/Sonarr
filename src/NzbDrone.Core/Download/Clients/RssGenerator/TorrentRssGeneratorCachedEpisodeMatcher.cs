using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Disk.Abstractions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.RssGenerator {

    public interface ITorrentRssGeneratorCachedEpisodeMatcher {
        bool Matches(TorrentRssGeneratorCachedEpisode cached, IFileSystemInfo fileSystemInfo);
    }
    public class TorrentRssGeneratorCachedEpisodeMatcher : ITorrentRssGeneratorCachedEpisodeMatcher {
        public bool Matches(TorrentRssGeneratorCachedEpisode cached, IFileSystemInfo fileSystemInfo) {
            if (cached == null || cached.Title.IsNullOrWhiteSpace() || fileSystemInfo == null || !fileSystemInfo.Exists)
                return false;

            var fiparsed = Parser.Parser.ParseTitle(fileSystemInfo.LogicalName);
            if (fiparsed == null)
                return false;

            var episodeInfo = cached.EpisodeInfo ??
                              Parser.Parser.ParseTitle(cached.Title);

            return fiparsed.ToString() == episodeInfo.ToString();
        }
    }
}
