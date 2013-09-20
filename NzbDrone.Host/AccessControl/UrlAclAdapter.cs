using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IUrlAclAdapter
    {
        void RefreshRegistration();
        bool IsRegistered();
        string UrlAcl { get; }
        string LocalUrlAcl { get; }
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private const string URL_ACL = "http://{0}:{1}/";

        private readonly IProcessProvider _processProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public UrlAclAdapter(IProcessProvider processProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _processProvider = processProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public bool IsRegistered()
        {
            var arguments = String.Format("http show urlacl {0}", UrlAcl);
            var output = RunNetsh(arguments);

            if (output == null || !output.Standard.Any()) return false;

            return output.Standard.Any(line => line.Contains(UrlAcl));
        }

        public string UrlAcl
        {
            get
            {
                return String.Format(URL_ACL, "*", _configFileProvider.Port);
            }
        }

        public string LocalUrlAcl
        {
            get
            {
                return String.Format(URL_ACL, "localhost", _configFileProvider.Port);
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

        private ProcessOutput RunNetsh(string arguments)
        {
            try
            {
                var output = _processProvider.StartAndCapture("netsh.exe", arguments);

                return output;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Error executing netsh with arguments: " + arguments, ex);
            }

            return null;
        }
    }
}