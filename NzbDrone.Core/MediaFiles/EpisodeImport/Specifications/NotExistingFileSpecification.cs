using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotExistingFileSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public NotExistingFileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason { get { return "Existing File"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            var episodeFiles = localEpisode.Episodes.Where(e => e.EpisodeFileId > 0).Select(e => e.EpisodeFile.Value);

            foreach (var episodeFile in episodeFiles)
            {
                if (Path.GetFileName(episodeFile.Path) == Path.GetFileName(localEpisode.Path) &&
                    episodeFile.Size == localEpisode.Size)
                {
                    _logger.Trace("File is a match for an existing episode file: {0}", localEpisode.Path);
                    return false;
                }
            }

            return true;
        }
    }
}
