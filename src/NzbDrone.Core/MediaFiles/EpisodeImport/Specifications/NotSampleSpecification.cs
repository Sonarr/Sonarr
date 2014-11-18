using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly ISampleService _sampleService;
        private readonly Logger _logger;

        public NotSampleSpecification(ISampleService sampleService,
                                      Logger logger)
        {
            _sampleService = sampleService;
            _logger = logger;
        }

        public string RejectionReason { get { return "Sample"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return true;
            }

            return !_sampleService.IsSample(localEpisode.Series,
                                            localEpisode.Quality,
                                            localEpisode.Path,
                                            localEpisode.Size,
                                            localEpisode.SeasonNumber);
        }
    }
}
