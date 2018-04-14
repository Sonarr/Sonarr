using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFileName : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalEpisode localEpisode)
        {
            var quality = localEpisode.FileEpisodeInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            var confidence = quality.QualityDetectionSource == QualityDetectionSource.Extension
                ? Confidence.Fallback
                : Confidence.Tag;

            return new AugmentQualityResult(quality.Quality.Source,
                                            confidence,
                                            quality.Quality.Resolution,
                                            confidence,
                                            quality.Revision);
        }
    }
}
