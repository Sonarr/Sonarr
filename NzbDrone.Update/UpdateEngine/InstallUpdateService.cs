using System;
using System.IO;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IInstallUpdateService
    {
        void Start(string installationFolder);
    }

    public class InstallUpdateService : IInstallUpdateService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectApplicationType _detectApplicationType;
        private readonly ITerminateNzbDrone _terminateNzbDrone;
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IBackupAndRestore _backupAndRestore;
        private readonly IStartNzbDrone _startNzbDrone;
        private readonly Logger _logger;

        public InstallUpdateService(IDiskProvider diskProvider, IDetectApplicationType detectApplicationType, ITerminateNzbDrone terminateNzbDrone,
            IProcessProvider processProvider, IEnvironmentProvider environmentProvider, IBackupAndRestore backupAndRestore, IStartNzbDrone startNzbDrone, Logger logger)
        {
            _diskProvider = diskProvider;
            _detectApplicationType = detectApplicationType;
            _terminateNzbDrone = terminateNzbDrone;
            _environmentProvider = environmentProvider;
            _backupAndRestore = backupAndRestore;
            _startNzbDrone = startNzbDrone;
            _logger = logger;
        }

        private void Verify(string targetFolder)
        {
            _logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(targetFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist " + targetFolder);

            _logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_environmentProvider.GetUpdatePackageFolder()))
                throw new DirectoryNotFoundException("Update folder doesn't exist " + _environmentProvider.GetUpdatePackageFolder());
        }

        public void Start(string installationFolder)
        {
            Verify(installationFolder);

            var appType = _detectApplicationType.GetAppType();

            try
            {
                _terminateNzbDrone.Terminate();

                _backupAndRestore.BackUp(installationFolder);

                _logger.Info("Moving update package to target");

                try
                {
                    _diskProvider.CopyDirectory(_environmentProvider.GetUpdatePackageFolder(), installationFolder);
                }
                catch (Exception e)
                {
                    _backupAndRestore.Restore(installationFolder);
                    _logger.FatalException("Failed to copy upgrade package to target folder.", e);
                }

            }
            finally
            {
                _startNzbDrone.Start(appType, installationFolder);
            }

        }
    }
}
