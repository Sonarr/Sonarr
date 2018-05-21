using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFolder : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalEpisode localEpisode)
        {
            var quality = localEpisode.FolderEpisodeInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            return new AugmentQualityResult(quality.Quality.Source,
                                            Confidence.Tag,
                                            quality.Quality.Resolution,
                                            Confidence.Tag,
                                            quality.Revision);
        }
    }
}
