using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles
{
    public interface ISetMediaFilePermissions
    {
        void SetPermissions(string filename);
    }

    public class SetMediaFilePermissions : ISetMediaFilePermissions
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public SetMediaFilePermissions(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public void SetPermissions(string filename)
        {
            if (OsInfo.IsWindows)
            {
                //Wrapped in Try/Catch to prevent this from causing issues with remote NAS boxes, the move worked, which is more important.
                try
                {
                    _diskProvider.InheritFolderPermissions(filename);
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException || ex is InvalidOperationException)
                    {
                        _logger.Debug("Unable to apply folder permissions to: ", filename);
                        _logger.TraceException(ex.Message, ex);
                    }

                    else
                    {
                        throw;
                    }
                }
            }

            else
            {
                _diskProvider.SetPermissions(filename, _configService.FileChmod);
            }
        }
    }
}
