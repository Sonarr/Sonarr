using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.Host
{
    public interface IUrlAclAdapter
    {
        void RefreshRegistration();
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private readonly IProcessProvider _processProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public UrlAclAdapter(IProcessProvider processProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _processProvider = processProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void RefreshRegistration()
        {
            if (OsInfo.Version.Major < 6)
                return;

            RegisterUrl(_configFileProvider.Port);
        }


        private void RegisterUrl(int portNumber)
        {
            var arguments = String.Format("http add urlacl http://*:{0}/ user=EVERYONE", portNumber);
            RunNetsh(arguments);
        }

        private void RunNetsh(string arguments)
        {
            try
            {


                var process = _processProvider.ShellExecute("netsh.exe", arguments);
                process.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Error executing netsh with arguments: " + arguments, ex);
            }
        }
    }
}