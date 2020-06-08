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
        private readonly List<IAugmentQuality> _augmentQualities;
        private readonly Logger _logger;

        public AggregateQuality(IEnumerable<IAugmentQuality> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities.OrderBy(a => a.Order).ToList();
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            var source = QualitySource.Unknown;
            var sourceConfidence = Confidence.Default;
            var resolution = 0;
            var resolutionConfidence = Confidence.Default;
            var revision = new Revision();

            foreach (var augmentQuality in _augmentQualities)
            {
                var augmentedQuality = augmentQuality.AugmentQuality(localEpisode, downloadClientItem);
                if (augmentedQuality == null)
                {
                    continue;
                }

                _logger.Trace("Considering Source {0} ({1}) Resolution {2} ({3}) Revision {4} from {5}", augmentedQuality.Source, augmentedQuality.SourceConfidence, augmentedQuality.Resolution, augmentedQuality.ResolutionConfidence, augmentedQuality.Revision, augmentQuality.Name);

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

            _logger.Trace("Selected Source {0} ({1}) Resolution {2} ({3}) Revision {4}", source, sourceConfidence, resolution, resolutionConfidence, revision);

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
