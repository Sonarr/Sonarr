using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class SameFileSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SameFileSpecification(Logger logger)
        {
            _logger = logger;
        }
        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var episodeFiles = localEpisode.Episodes.Where(e => e.EpisodeFileId != 0).Select(e => e.EpisodeFile).ToList();

            if (episodeFiles.Count == 0)
            {
                _logger.Debug("No existing episode file, skipping");
                return Decision.Accept();
            }

            if (episodeFiles.Count > 1)
            {
                _logger.Debug("More than one existing episode file, skipping.");
                return Decision.Accept();
            }

            var episodeFile = episodeFiles.First().Value;

            if (episodeFile == null)
            {
                var episode = localEpisode.Episodes.First();
                _logger.Trace("Unable to get episode file details from the DB. EpisodeId: {0} EpisodeFileId: {1}", episode.Id, episode.EpisodeFileId);

                return Decision.Accept();
            }

            if (episodeFiles.First().Value.Size == localEpisode.Size)
            {
                _logger.Debug("'{0}' Has the same filesize as existing file", localEpisode.Path);
                return Decision.Reject("Has the same filesize as existing file");
            }

            return Decision.Accept();
        }
    }
}
