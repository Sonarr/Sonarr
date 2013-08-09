using System;
using System.IO;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

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

            if (Path.GetExtension(localEpisode.Path).Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Trace("Skipping smaple check for .flv file");
                return true;
            }

            if (localEpisode.Size > SampleSizeLimit)
            {
                return true;
            }

            var runTime = _videoFileInfoReader.GetRunTime(localEpisode.Path);

            if (runTime.TotalMinutes.Equals(0))
            {
                _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", localEpisode);
                return false;
            }

            if (runTime.TotalMinutes < 3)
            {
                _logger.Trace("[{0}] appears to be a sample. Size: {1} Runtime: {2}", localEpisode.Path, localEpisode.Size, runTime);
                return false;
            }

            return true;
        }
    }
}
