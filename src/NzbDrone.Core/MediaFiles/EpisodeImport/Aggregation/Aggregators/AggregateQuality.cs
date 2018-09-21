using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateQuality : IAggregateLocalEpisode
    {
        private readonly IEnumerable<IAugmentQuality> _augmentQualities;
        private readonly Logger _logger;

        public AggregateQuality(IEnumerable<IAugmentQuality> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities;
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, bool otherFiles)
        {
            var augmentedQualities = _augmentQualities.Select(a => a.AugmentQuality(localEpisode))
                                                      .Where(a => a != null)
                                                      .OrderBy(a => a.SourceConfidence);

            var source = QualitySource.Unknown;
            var sourceConfidence = Confidence.Default;
            var resolution = 0;
            var resolutionConfidence = Confidence.Default;
            var revison = new Revision();

            foreach (var augmentedQuality in augmentedQualities)
            {
                if (augmentedQuality.Source > source ||
                    augmentedQuality.SourceConfidence > sourceConfidence && augmentedQuality.Source != QualitySource.Unknown)
                {
                    source = augmentedQuality.Source;
                    sourceConfidence = augmentedQuality.SourceConfidence;
                }

                if (augmentedQuality.Resolution > resolution ||
                    augmentedQuality.ResolutionConfidence > resolutionConfidence && augmentedQuality.Resolution > 0)
                {
                    resolution = augmentedQuality.Resolution;
                    resolutionConfidence = augmentedQuality.ResolutionConfidence;
                }

                if (augmentedQuality.Revision != null && augmentedQuality.Revision > revison)
                {
                    revison = augmentedQuality.Revision;
                }
            }

            _logger.Trace("Finding quality. Source: {0}. Resolution: {1}", source, resolution);

            var quality = new QualityModel(QualityFinder.FindBySourceAndResolution(source, resolution), revison);

            if (resolutionConfidence == Confidence.MediaInfo)
            {
                quality.QualityDetectionSource = QualityDetectionSource.MediaInfo;
            }
            else if (sourceConfidence == Confidence.Fallback || resolutionConfidence == Confidence.Fallback)
            {
                quality.QualityDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.QualityDetectionSource = QualityDetectionSource.Name;
            }

            _logger.Debug("Using quality: {0}", quality);

            localEpisode.Quality = quality;

            return localEpisode;
        }
    }
}
