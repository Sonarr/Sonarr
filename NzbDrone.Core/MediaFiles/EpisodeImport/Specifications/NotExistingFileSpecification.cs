using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotExistingFileSpecification : IImportDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public NotExistingFileSpecification(IMediaFileService mediaFileService, Logger logger)
        {
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public string RejectionReason { get { return "Existing File"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (_mediaFileService.Exists(localEpisode.Path))
            {
                _logger.Trace("File is a match for an existing episode file: {0}", localEpisode.Path);
                return false;
            }

            var existingFiles = localEpisode.Episodes.Where(e => e.EpisodeFileId > 0).Select(e => e.EpisodeFile.Value);

            foreach (var existingFile in existingFiles)
            {
                if (Path.GetFileName(existingFile.Path) == Path.GetFileName(localEpisode.Path) &&
                    existingFile.Size == localEpisode.Size)
                {
                    _logger.Trace("File is a match for an existing episode file: {0}", localEpisode.Path);
                    return false;
                }
            }

            return true;
        }
    }
}
