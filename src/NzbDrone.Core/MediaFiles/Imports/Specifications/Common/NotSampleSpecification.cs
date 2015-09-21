using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Common
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

        public Decision IsSatisfiedBy(LocalItem localItem)
        {
            if (localItem.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return Decision.Accept();
            }

            var sample = false;

            if (localItem.Media != null)
            {
                sample = _detectSample.IsSample(localItem.Media,
                                                localItem.Quality,
                                                localItem.Path,
                                                localItem.Size);
            }

            if (sample)
            {
                return Decision.Reject("Sample");
            }

            return Decision.Accept();
        }
    }
}
