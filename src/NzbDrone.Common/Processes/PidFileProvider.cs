﻿using System;
using System.IO;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Processes
{
    public interface IProvidePidFile
    {
        void Write();
    }

    public class PidFileProvider : IProvidePidFile
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IProcessProvider _processProvider;
        private readonly IStartupContext _startupContext;
        private readonly Logger _logger;

        public PidFileProvider(IAppFolderInfo appFolderInfo, IProcessProvider processProvider, IStartupContext startupContext, Logger logger)
        {
            _appFolderInfo = appFolderInfo;
            _processProvider = processProvider;
            _startupContext = startupContext;
            _logger = logger;
        }

        public void Write()
        {
            if (OsInfo.IsWindows)
            {
                return;
            }

            string filename;

            if (_startupContext.Args.ContainsKey(StartupContext.PID_FILE))
            {
                filename = _startupContext.Args[StartupContext.PID_FILE];
                _logger.Info("Using custom location for PID file: " + filename);
            }
            else
            {
                filename = Path.Combine(_appFolderInfo.AppDataFolder, "nzbdrone.pid");
            }

            try
            {
                File.WriteAllText(filename, _processProvider.GetCurrentProcess().Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to write PID file: " + filename, ex);
                throw;
            }
        }
    }
}
