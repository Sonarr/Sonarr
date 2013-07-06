using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotAlreadyImportedSpecification : IImportDecisionEngineSpecification
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public NotAlreadyImportedSpecification(IMediaFileService mediaFileService, Logger logger)
        {
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public string RejectionReason { get { return "Is Sample"; } }

        public bool IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (_mediaFileService.Exists(localEpisode.Path))
            {
                _logger.Trace("[{0}] already exists in the database. skipping.", localEpisode.Path);
                return false;
            }

            return true;
        }
    }
}
