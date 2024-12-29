using NLog;
using Workarr.Configuration;
using Workarr.Disk;
using Workarr.Download;
using Workarr.EnvironmentInfo;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
{
    public class NotUnpackingSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NotUnpackingSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug((string)"{0} is in series folder, skipping unpacking check", (string)localEpisode.Path);
                return ImportSpecDecision.Accept();
            }

            foreach (var workingFolder in _configService.DownloadClientWorkingFolders.Split('|'))
            {
                var parent = Directory.GetParent(localEpisode.Path);
                while (parent != null)
                {
                    if (parent.Name.StartsWith(workingFolder))
                    {
                        if (OsInfo.IsNotWindows)
                        {
                            _logger.Debug((string)"{0} is still being unpacked", (string)localEpisode.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }

                        if (_diskProvider.FileGetLastWrite(localEpisode.Path) > DateTime.UtcNow.AddMinutes(-5))
                        {
                            _logger.Debug((string)"{0} appears to be unpacking still", (string)localEpisode.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }
                    }

                    parent = parent.Parent;
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
