using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class SplitEpisodeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.FileEpisodeInfo == null)
            {
                return Decision.Accept();
            }

            if (localEpisode.FileEpisodeInfo.IsSplitEpisode)
            {
                _logger.Debug("Single episode split into multiple files");
                return Decision.Reject("Single episode split into multiple files");
            }

            return Decision.Accept();
        }
    }
}
