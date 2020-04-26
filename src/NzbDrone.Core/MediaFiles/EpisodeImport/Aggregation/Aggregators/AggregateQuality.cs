using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
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

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            var augmentedQualities = _augmentQualities.OrderBy(a => a.Order)
                                                      .Select(a => a.AugmentQuality(localEpisode, downloadClientItem))
                                                      .Where(a => a != null)
                                                      .ToList();

            var source = QualitySource.Unknown;
            var sourceConfidence = Confidence.Default;
            var resolution = 0;
            var resolutionConfidence = Confidence.Default;
            var revision = new Revision();

            foreach (var augmentedQuality in augmentedQualities)
            {
                if (source == QualitySource.Unknown ||
                    augmentedQuality.SourceConfidence > sourceConfidence && augmentedQuality.Source != QualitySource.Unknown)
                {
                    source = augmentedQuality.Source;
                    sourceConfidence = augmentedQuality.SourceConfidence;
                }

                if (resolution == 0 ||
                    augmentedQuality.ResolutionConfidence > resolutionConfidence && augmentedQuality.Resolution > 0)
                {
                    resolution = augmentedQuality.Resolution;
                    resolutionConfidence = augmentedQuality.ResolutionConfidence;
                }

                if (augmentedQuality.Revision != null && augmentedQuality.Revision > revision)
                {
                    revision = augmentedQuality.Revision;
                }
            }

            _logger.Trace("Finding quality. Source: {0}. Resolution: {1}", source, resolution);

            var quality = new QualityModel(QualityFinder.FindBySourceAndResolution(source, resolution), revision);

            if (resolutionConfidence == Confidence.MediaInfo)
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.MediaInfo;
            }
            else if (resolutionConfidence == Confidence.Fallback)
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.Name;
            }

            if (sourceConfidence == Confidence.Fallback)
            {
                quality.SourceDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.SourceDetectionSource = QualityDetectionSource.Name;
            }

            _logger.Debug("Using quality: {0}", quality);

            localEpisode.Quality = quality;

            return localEpisode;
        }
    }
}
