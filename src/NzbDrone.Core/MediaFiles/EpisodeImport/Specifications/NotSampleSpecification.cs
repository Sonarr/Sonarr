using NLog;
using NzbDrone.Core.DecisionEngine;
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

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return Decision.Accept();
            }

            var sample = _detectSample.IsSample(localEpisode.Series,
                                                localEpisode.Quality,
                                                localEpisode.Path,
                                                localEpisode.Size,
                                                localEpisode.IsSpecial);

            if (sample)
            {
                return Decision.Reject("Sample");
            }

            return Decision.Accept();
        }
    }
}
