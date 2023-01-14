using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromMediaInfo : IAugmentQuality
    {
        private readonly Logger _logger;

        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentQualityFromMediaInfo(Logger logger)
        {
            _logger = logger;
        }

        public AugmentQualityResult AugmentQuality(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.MediaInfo == null)
            {
                return null;
            }

            var width = localEpisode.MediaInfo.Width;
            var height = localEpisode.MediaInfo.Height;
            var source = QualitySource.Unknown;
            var sourceConfidence = Confidence.Default;
            var title = localEpisode.MediaInfo.Title;

            if (title.IsNotNullOrWhiteSpace())
            {
                var parsedQuality = QualityParser.ParseQualityName(title.Trim());

                // Only use the quality if it's not unknown and the source is from the name (which is MediaInfo's title in this case)
                if (parsedQuality.Quality.Source != QualitySource.Unknown &&
                    parsedQuality.SourceDetectionSource == QualityDetectionSource.Name)
                {
                    source = parsedQuality.Quality.Source;
                    sourceConfidence = Confidence.MediaInfo;
                }
            }

            if (width >= 3200 || height >= 2100)
            {
                _logger.Trace("Resolution {0}x{1} considered 2160p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, 2160, Confidence.MediaInfo);
            }

            if (width >= 1800 || height >= 1000)
            {
                _logger.Trace("Resolution {0}x{1} considered 1080p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, 1080, Confidence.MediaInfo);
            }

            if (width >= 1200 || height >= 700)
            {
                _logger.Trace("Resolution {0}x{1} considered 720p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, 720, Confidence.MediaInfo);
            }

            if (width > 0 && height > 0)
            {
                _logger.Trace("Resolution {0}x{1} considered 480p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, 480, Confidence.MediaInfo);
            }

            _logger.Trace("Resolution {0}x{1}", width, height);

            return null;
        }
    }
}
