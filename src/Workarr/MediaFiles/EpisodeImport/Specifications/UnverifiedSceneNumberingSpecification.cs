using NLog;
using Workarr.Download;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
{
    public class UnverifiedSceneNumberingSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UnverifiedSceneNumberingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("Skipping scene numbering check for existing episode");
                return ImportSpecDecision.Accept();
            }

            if (localEpisode.Episodes.Any(v => v.UnverifiedSceneNumbering))
            {
                _logger.Debug((string)"This file uses unverified scene numbers, will not auto-import until numbering is confirmed on TheXEM. Skipping {0}", (string)localEpisode.Path);
                return ImportSpecDecision.Reject(ImportRejectionReason.UnverifiedSceneMapping, "This show has individual episode mappings on TheXEM but the mapping for this episode has not been confirmed yet by their administrators. TheXEM needs manual input.");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
