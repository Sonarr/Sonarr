using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Common
{
    public class FreeSpaceSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IDiskProvider diskProvider, IConfigService configService,  Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalItem localItem)
        {
            if (_configService.SkipFreeSpaceCheckWhenImporting)
            {
                _logger.Debug("Skipping free space check when importing");
                return Decision.Accept();
            }

            try
            {
                if (localItem.ExistingFile)
                {
                    _logger.Debug("Skipping free space check for existing episode");
                    return Decision.Accept();
                }

                if (localItem.Media == null)
                {
                    _logger.Debug("No serie or movie info");
                    return Decision.Reject("Shouldn't happend: No serie or movie info");
                }
                
                var path = Directory.GetParent(localItem.Media.Path);


                var freeSpace = _diskProvider.GetAvailableSpace(path.FullName);

                if (!freeSpace.HasValue)
                {
                    _logger.Debug("Free space check returned an invalid result for: {0}", path);
                    return Decision.Accept();
                }

                if (freeSpace < localItem.Size + 100.Megabytes())
                {
                    _logger.Warn("Not enough free space ({0}) to import: {1} ({2})", freeSpace, localItem, localItem.Size);
                    return Decision.Reject("Not enough free space");
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error("Unable to check free disk space while importing. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to check free disk space while importing: " + localItem.Path, ex);
            }

            return Decision.Accept();
        }
    }
}
