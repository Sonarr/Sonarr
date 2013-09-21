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
        void ConfigureUrl();
        string UrlAcl { get; }
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private const string URL_ACL = "http://{0}:{1}/";

        private readonly IProcessProvider _processProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public string UrlAcl { get; private set; }
        private string _localUrl;
        private string _wildcardUrl;

        public UrlAclAdapter(IProcessProvider processProvider,
                             IConfigFileProvider configFileProvider,
                             IRuntimeInfo runtimeInfo,
                             Logger logger)
        {
            _processProvider = processProvider;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _logger = logger;

             _localUrl = String.Format(URL_ACL, "localhost", _configFileProvider.Port);
             _wildcardUrl = String.Format(URL_ACL, "*", _configFileProvider.Port);

            UrlAcl = _wildcardUrl;
        }

        public void ConfigureUrl()
        {
            if (!_runtimeInfo.IsAdmin && !IsRegistered)
            {
                UrlAcl = _localUrl;
            }

            if (_runtimeInfo.IsAdmin)
            {
                RefreshRegistration();
            }
        }

        private void RefreshRegistration()
        {
            if (OsInfo.Version.Major < 6)
                return;

            RegisterUrl();
        }

        private bool IsRegistered
        {
            get
            {
                var arguments = String.Format("http show urlacl {0}", _wildcardUrl);
                var output = RunNetsh(arguments);

                if (output == null || !output.Standard.Any()) return false;

                return output.Standard.Any(line => line.Contains(_wildcardUrl));
            }
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