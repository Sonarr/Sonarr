using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common;
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

            var runTime = _videoFileInfoReader.GetRunTime(localEpisode.Path);

            if (localEpisode.Size < SampleSizeLimit && runTime.TotalMinutes < 3)
            {
                _logger.Trace("[{0}] appears to be a sample.", localEpisode.Path);
                return false;
            }

            return true;
        }
    }
}
