using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FreeSpaceSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (_configService.SkipFreeSpaceCheckWhenImporting)
            {
                _logger.Debug("Skipping free space check when importing");
                return ImportSpecDecision.Accept();
            }

            try
            {
                if (localEpisode.ExistingFile)
                {
                    _logger.Debug("Skipping free space check for existing episode");
                    return ImportSpecDecision.Accept();
                }

                var path = Directory.GetParent(localEpisode.Series.Path);
                var freeSpace = _diskProvider.GetAvailableSpace(path.FullName);

                if (!freeSpace.HasValue)
                {
                    _logger.Debug("Free space check returned an invalid result for: {0}", path);
                    return ImportSpecDecision.Accept();
                }

                if (freeSpace < localEpisode.Size + _configService.MinimumFreeSpaceWhenImporting.Megabytes())
                {
                    _logger.Warn("Not enough free space ({0}) to import: {1} ({2})", freeSpace, localEpisode, localEpisode.Size);
                    return ImportSpecDecision.Reject(ImportRejectionReason.MinimumFreeSpace, "Not enough free space");
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error(ex, "Unable to check free disk space while importing.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to check free disk space while importing. {0}", localEpisode.Path);
            }

            return ImportSpecDecision.Accept();
        }
    }
}
