using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseHash : IAggregateLocalEpisode
    {
        public int Order => 1;

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var releaseHash = GetReleaseHash(localEpisode.FileEpisodeInfo);

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localEpisode.DownloadClientEpisodeInfo);
            }

            if (releaseHash.IsNullOrWhiteSpace())
            {
                releaseHash = GetReleaseHash(localEpisode.FolderEpisodeInfo);
            }

            localEpisode.ReleaseHash = releaseHash;

            return localEpisode;
        }

        private string GetReleaseHash(ParsedEpisodeInfo episodeInfo)
        {
            // ReleaseHash doesn't make sense for a FullSeason, since hashes should be specific to a file
            if (episodeInfo == null || episodeInfo.FullSeason)
            {
                return null;
            }

            return episodeInfo.ReleaseHash;
        }
    }
}
