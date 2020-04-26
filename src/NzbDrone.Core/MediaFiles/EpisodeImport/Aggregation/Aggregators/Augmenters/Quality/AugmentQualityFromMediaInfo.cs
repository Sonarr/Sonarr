using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromMediaInfo : IAugmentQuality
    {
        public int Order => 4;

        public AugmentQualityResult AugmentQuality(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.MediaInfo == null)
            {
                return null;
            }

            var width = localEpisode.MediaInfo.Width;

            if (width >= 3200)
            {
                return AugmentQualityResult.ResolutionOnly(2160, Confidence.MediaInfo);
            }

            if (width >= 1800)
            {
                return AugmentQualityResult.ResolutionOnly(1080, Confidence.MediaInfo);
            }

            if (width >= 1200)
            {
                return AugmentQualityResult.ResolutionOnly(720, Confidence.MediaInfo);
            }

            if (width > 0)
            {
                return AugmentQualityResult.ResolutionOnly(480, Confidence.MediaInfo);
            }

            return null;
        }
    }
}
