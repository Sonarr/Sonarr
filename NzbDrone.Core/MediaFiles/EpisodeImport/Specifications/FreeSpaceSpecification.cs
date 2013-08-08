using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FreeSpaceSpecification : IImportDecisionEngineSpecification
    {
        private readonly IBuildFileNames _buildFileNames;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IBuildFileNames buildFileNames, IDiskProvider diskProvider,  Logger logger)
        {
            _buildFileNames = buildFileNames;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public string RejectionReason { get { return "Not enough free space"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            var newFileName = Path.GetFileNameWithoutExtension(localEpisode.Path);
            var destinationFilename = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));
            
            var freeSpace = _diskProvider.GetAvilableSpace(destinationFilename);

            if (freeSpace < localEpisode.Size)
            {
                _logger.Warn("Not enough free space to import: {0}", localEpisode);
                return false;
            }

            return true;
        }
    }
}
