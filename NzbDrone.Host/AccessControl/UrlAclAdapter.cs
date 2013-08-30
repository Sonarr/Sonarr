using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IUrlAclAdapter
    {
        void RefreshRegistration();
        string UrlAcl { get; }
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

        public string UrlAcl
        {
            get
            {
                return "http://*:" + _configFileProvider.Port + "/";
            }
        }

        public void RefreshRegistration()
        {
            if (OsInfo.Version.Major < 6)
                return;

            RegisterUrl();
        }

        private void RegisterUrl()
        {
            var arguments = String.Format("http add urlacl {0} sddl=D:(A;;GX;;;S-1-1-0)", UrlAcl);
            RunNetsh(arguments);
        }

        private void RunNetsh(string arguments)
        {
            try
            {
                var process = _processProvider.Start("netsh.exe", arguments);
                process.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Error executing netsh with arguments: " + arguments, ex);
            }
        }
    }
}