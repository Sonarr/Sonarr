using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregateLocalEpisode
    {
        public LocalEpisode Aggregate(LocalEpisode localEpisode, bool otherFiles)
        {
            var releaseGroup = localEpisode.DownloadClientEpisodeInfo?.ReleaseGroup;

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localEpisode.FolderEpisodeInfo?.ReleaseGroup;
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localEpisode.FileEpisodeInfo?.ReleaseGroup;
            }

            localEpisode.ReleaseGroup = releaseGroup;

            return localEpisode;
        }
    }
}
