using System;
using NLog;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public interface IBrowserService
    {
        void LaunchWebUI();
    }

    public class BrowserService : IBrowserService
    {
        private readonly IProcessProvider _processProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public BrowserService(IProcessProvider processProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _processProvider = processProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void LaunchWebUI()
        {
            var url = string.Format("http://localhost:{0}", _configFileProvider.Port);
            try
            {
                _logger.Info("Starting default browser. {0}", url);
                _processProvider.OpenDefaultBrowser(url);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't open defult browser to " + url, e);
            }
        }
    }
}