using System;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public NotSampleSpecification(IDetectSample detectSample,
                                      Logger logger)
        {
            _detectSample = detectSample;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return Decision.Accept();
            }

            try
            {
                var sample = _detectSample.IsSample(localEpisode.Series, localEpisode.Path, localEpisode.IsSpecial);

                if (sample == DetectSampleResult.Sample)
                {
                    return Decision.Reject("Sample");
                }
                else if (sample == DetectSampleResult.Indeterminate)
                {
                    return Decision.Reject("Unable to determine if file is a sample");
                }
            }
            catch (InvalidSeasonException e)
            {
                _logger.Warn(e, "Invalid season detected during sample check");
            }

            return Decision.Accept();
        }
    }
}
