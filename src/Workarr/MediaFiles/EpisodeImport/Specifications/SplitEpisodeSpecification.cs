using NLog;
using Workarr.Download;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
{
    public class SplitEpisodeSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.FileEpisodeInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            if (localEpisode.FileEpisodeInfo.IsSplitEpisode)
            {
                _logger.Debug("Single episode split into multiple files");
                return ImportSpecDecision.Reject(ImportRejectionReason.SplitEpisode, "Single episode split into multiple files");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
