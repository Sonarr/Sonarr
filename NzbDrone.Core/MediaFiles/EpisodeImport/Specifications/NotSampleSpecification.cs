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
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public NotSampleSpecification(IDiskProvider diskProvider, IVideoFileInfoReader videoFileInfoReader, Logger logger)
        {
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
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

            var size = _diskProvider.GetFileSize(localEpisode.Path);
            var runTime = _videoFileInfoReader.GetRunTime(localEpisode.Path);

            if (size < Constants.IgnoreFileSize && runTime.TotalMinutes < 3)
            {
                _logger.Trace("[{0}] appears to be a sample.", localEpisode.Path);
                return false;
            }

            return true;
        }
    }
}
