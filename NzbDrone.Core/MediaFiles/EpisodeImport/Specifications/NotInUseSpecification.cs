using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotInUseSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public NotInUseSpecification(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public string RejectionReason { get { return "File is in use"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (_diskProvider.IsParent(localEpisode.Series.Path, localEpisode.Path))
            {
                _logger.Trace("{0} is in series folder, skipping in use check", localEpisode.Path);
                return true;
            }

            if (_diskProvider.IsFileLocked(localEpisode.Path))
            {
                _logger.Trace("{0} is in use");
                return false;
            }

            return true;
        }
    }
}
