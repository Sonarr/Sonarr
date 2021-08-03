using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregateLocalEpisode
    {
        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            // Prefer ReleaseGroup from DownloadClient/Folder if they're not a season pack
            var releaseGroup = GetReleaseGroup(localEpisode.DownloadClientEpisodeInfo, true);

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localEpisode.FolderEpisodeInfo, true);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localEpisode.FileEpisodeInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localEpisode.DownloadClientEpisodeInfo, false);
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = GetReleaseGroup(localEpisode.FolderEpisodeInfo, false);
            }

            localEpisode.ReleaseGroup = releaseGroup;

            return localEpisode;
        }

        private string GetReleaseGroup(ParsedEpisodeInfo episodeInfo, bool skipFullSeason)
        {
            if (episodeInfo == null || (episodeInfo.FullSeason && skipFullSeason))
            {
                return null;
            }

            return episodeInfo.ReleaseGroup;
        }
    }
}
