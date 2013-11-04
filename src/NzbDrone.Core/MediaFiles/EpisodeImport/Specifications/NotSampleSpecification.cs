using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;
        private static List<Quality> _largeSampleSizeQualities = new List<Quality> { Quality.HDTV1080p, Quality.WEBDL1080p, Quality.Bluray1080p };

        public NotSampleSpecification(IVideoFileInfoReader videoFileInfoReader,
                                      Logger logger)
        {
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public static long SampleSizeLimit
        {
            get
            {
                return 70.Megabytes();
            }
        }

        public string RejectionReason { get { return "Sample"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Trace("Existing file, skipping sample check");
                return true;
            }

            if (localEpisode.Series.SeriesType == SeriesTypes.Daily)
            {
                _logger.Trace("Daily Series, skipping sample check");
                return true;
            }

            if (localEpisode.SeasonNumber == 0)
            {
                _logger.Trace("Special, skipping sample check");
                return true;
            }

            var extension = Path.GetExtension(localEpisode.Path);

            if (extension != null && extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Trace("Skipping sample check for .flv file");
                return true;
            }

            if (OsInfo.IsWindows)
            {
                var runTime = _videoFileInfoReader.GetRunTime(localEpisode.Path);

                if (runTime.TotalMinutes.Equals(0))
                {
                    _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", localEpisode);
                    return false;
                }

                if (runTime.TotalSeconds < 90)
                {
                    _logger.Trace("[{0}] appears to be a sample. Size: {1} Runtime: {2}", localEpisode.Path, localEpisode.Size, runTime);
                    return false;
                }

                _logger.Trace("Runtime is over 2 minutes, skipping file size check");
                return true;
            }

            return CheckSize(localEpisode);
        }

        private bool CheckSize(LocalEpisode localEpisode)
        {
            if (_largeSampleSizeQualities.Contains(localEpisode.Quality.Quality))
            {
                if (localEpisode.Size < SampleSizeLimit * 2)
                {
                    return false;
                }
            }

            if (localEpisode.Size < SampleSizeLimit)
            {
                return false;
            }

            return true;
        }
    }
}
