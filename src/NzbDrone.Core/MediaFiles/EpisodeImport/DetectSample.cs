using System;
using System.IO;
using NLog;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IDetectSample
    {
        DetectSampleResult IsSample(Series series, string path, bool isSpecial);
        DetectSampleResult IsSample(LocalEpisode localEpisode);
    }

    public class DetectSample : IDetectSample
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public DetectSample(IVideoFileInfoReader videoFileInfoReader, Logger logger)
        {
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public DetectSampleResult IsSample(Series series, string path, bool isSpecial)
        {
            var extensionResult = IsSample(path, isSpecial);

            if (extensionResult != DetectSampleResult.Indeterminate)
            {
                return extensionResult;
            }

            var fileRuntime = _videoFileInfoReader.GetRunTime(path);

            if (!fileRuntime.HasValue)
            {
                _logger.Error("Failed to get runtime from the file, make sure ffprobe is available");
                return DetectSampleResult.Indeterminate;
            }

            return IsSample(path, fileRuntime.Value, series.Runtime);
        }

        public DetectSampleResult IsSample(LocalEpisode localEpisode)
        {
            var extensionResult = IsSample(localEpisode.Path, localEpisode.IsSpecial);

            if (extensionResult != DetectSampleResult.Indeterminate)
            {
                return extensionResult;
            }

            var runtime = 0;

            foreach (var episode in localEpisode.Episodes)
            {
                runtime += episode.Runtime > 0 ? episode.Runtime : localEpisode.Series.Runtime;
            }

            if (localEpisode.MediaInfo == null)
            {
                _logger.Error("Failed to get runtime from the file, make sure ffprobe is available");
                return DetectSampleResult.Indeterminate;
            }

            return IsSample(localEpisode.Path, localEpisode.MediaInfo.RunTime, runtime);
        }

        private DetectSampleResult IsSample(string path, bool isSpecial)
        {
            if (isSpecial)
            {
                _logger.Debug("Special, skipping sample check");
                return DetectSampleResult.NotSample;
            }

            var extension = Path.GetExtension(path);

            if (extension != null && extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Debug("Skipping sample check for .flv file");
                return DetectSampleResult.NotSample;
            }

            if (extension != null && extension.Equals(".strm", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Debug("Skipping sample check for .strm file");
                return DetectSampleResult.NotSample;
            }

            return DetectSampleResult.Indeterminate;
        }

        private DetectSampleResult IsSample(string path, TimeSpan fileRuntime, int expectedRuntime)
        {
            var minimumRuntime = GetMinimumAllowedRuntime(expectedRuntime);

            if (fileRuntime.TotalMinutes.Equals(0))
            {
                _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", path);
                return DetectSampleResult.Sample;
            }

            if (fileRuntime.TotalSeconds < minimumRuntime)
            {
                _logger.Debug("[{0}] appears to be a sample. Runtime: {1} seconds. Expected at least: {2} seconds", path, fileRuntime, minimumRuntime);
                return DetectSampleResult.Sample;
            }

            _logger.Debug("[{0}] does not appear to be a sample. Runtime {1} seconds is more than minimum of {2} seconds", path, fileRuntime, minimumRuntime);
            return DetectSampleResult.NotSample;
        }

        private int GetMinimumAllowedRuntime(int runtime)
        {
            // Anime short - 15 seconds
            if (runtime <= 3)
            {
                return 15;
            }

            // Webisodes - 90 seconds
            if (runtime <= 10)
            {
                return 90;
            }

            // 30 minute episodes - 5 minutes
            if (runtime <= 30)
            {
                return 300;
            }

            // 60 minute episodes - 10 minutes
            return 600;
        }
    }
}
