using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileAttributeService
    {
        void SetFilePermissions(string path);
        void SetFolderPermissions(string path);
        void SetFolderLastWriteTime(string path, DateTime time);
    }

    public class MediaFileAttributeService : IMediaFileAttributeService
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public MediaFileAttributeService(IConfigService configService,
                                         IDiskProvider diskProvider,
                                         Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void SetFilePermissions(string path)
        {
            if (OsInfo.IsWindows)
            {
                //Wrapped in Try/Catch to prevent this from causing issues with remote NAS boxes
                try
                {
                    _diskProvider.InheritFolderPermissions(path);
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is FileNotFoundException)
                    {
                        _logger.Debug("Unable to apply folder permissions to {0}", path);
                        _logger.Debug(ex, ex.Message);
                    }
                    else
                    {
                        _logger.Warn("Unable to apply folder permissions to: {0}", path);
                        _logger.Warn(ex, ex.Message);
                    }
                }
            }
            else
            {
                SetMonoPermissions(path);
            }
        }

        public void SetFolderPermissions(string path)
        {
            if (OsInfo.IsNotWindows)
            {
                SetMonoPermissions(path);
            }
        }

        public void SetFolderLastWriteTime(string path, DateTime time)
        {
            if (OsInfo.IsWindows)
            {
                _logger.Debug("Setting last write time on series folder: {0}", path);
                _diskProvider.FolderSetLastWriteTime(path, time);
            }
        }

        private void SetMonoPermissions(string path)
        {
            if (!_configService.SetPermissionsLinux)
            {
                return;
            }

            try
            {
                _diskProvider.SetPermissions(path, _configService.ChmodFolder, _configService.ChownGroup);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to apply permissions to: " + path);
            }
        }
    }
}
