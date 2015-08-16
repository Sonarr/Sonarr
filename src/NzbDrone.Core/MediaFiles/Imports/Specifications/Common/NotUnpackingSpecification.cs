using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports.Specifications.Common
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

        public Decision IsSatisfiedBy(LocalItem localItem)
        {
            if (localItem.ExistingFile)
            {
                _logger.Debug("{0} is in series folder, unpacking check", localItem.Path);
                return Decision.Accept();
            }

            foreach (var workingFolder in _configService.DownloadClientWorkingFolders.Split('|'))
            {
                DirectoryInfo parent = Directory.GetParent(localItem.Path);
                while (parent != null)
                {
                    if (parent.Name.StartsWith(workingFolder))
                    {
                        if (OsInfo.IsNotWindows)
                        {
                            _logger.Debug("{0} is still being unpacked", localItem.Path);
                            return Decision.Reject("File is still being unpacked");
                        }

                        if (_diskProvider.FileGetLastWrite(localItem.Path) > DateTime.UtcNow.AddMinutes(-5))
                        {
                            _logger.Debug("{0} appears to be unpacking still", localItem.Path);
                            return Decision.Reject("File is still being unpacked");
                        }
                    }

                    parent = parent.Parent;
                }
            }

            return Decision.Accept();
        }
    }
}
