using System;
using System.IO;
using NLog;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IDetectSample
    {
        DetectSampleResult IsSample(Series series, string path, bool isSpecial, MediaInfoModel mediaInfo = null);
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

        public DetectSampleResult IsSample(Series series, string path, bool isSpecial, MediaInfoModel mediaInfo = null)
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

            if (mediaInfo == null)
            {
                mediaInfo = _videoFileInfoReader.GetMediaInfo(path);
            }

            if (mediaInfo == null)
            {
                _logger.Error("Failed to get runtime from the file, make sure mediainfo is available");
                return DetectSampleResult.Indeterminate;
            }

            if (mediaInfo.Format == "MZ")
            {
                _logger.Error("Mediainfo indicates this file is an executable");
                return DetectSampleResult.Executable;
            }

            var runtime = mediaInfo.RunTime;
            var minimumRuntime = GetMinimumAllowedRuntime(series);

            if (runtime.TotalMinutes.Equals(0))
            {
                _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", path);
                return DetectSampleResult.Sample;
            }

            if (runtime.TotalSeconds < minimumRuntime)
            {
                _logger.Debug("[{0}] appears to be a sample. Runtime: {1} seconds. Expected at least: {2} seconds", path, mediaInfo, minimumRuntime);
                return DetectSampleResult.Sample;
            }

            _logger.Debug("Runtime is over 90 seconds");
            return DetectSampleResult.NotSample;
        }

        private int GetMinimumAllowedRuntime(Series series)
        {
            //Anime short - 15 seconds
            if (series.Runtime <= 3)
            {
                return 15;
            }

            //Webisodes - 90 seconds
            if (series.Runtime <= 10)
            {
                return 90;
            }

            //30 minute episodes - 5 minutes
            if (series.Runtime <= 30)
            {
                return 300;
            }

            //60 minute episodes - 10 minutes
            return 600;
        }
    }
}
