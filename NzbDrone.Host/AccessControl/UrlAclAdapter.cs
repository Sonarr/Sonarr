using System;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IUrlAclAdapter
    {
        void ConfigureUrl();
        string Url { get; }
        string HttpsUrl { get; }
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private readonly INetshProvider _netshProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public string Url { get; private set; }
        public string HttpsUrl { get; private set; }

        private string _localUrl;
        private string _wildcardUrl;
        private string _localHttpsUrl;
        private string _wildcardHttpsUrl;

        public UrlAclAdapter(INetshProvider netshProvider,
                             IConfigFileProvider configFileProvider,
                             IRuntimeInfo runtimeInfo,
                             Logger logger)
        {
            _netshProvider = netshProvider;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _logger = logger;

            _localUrl = String.Format("http://localhost:{0}/", _configFileProvider.Port);
            _wildcardUrl = String.Format("http://*:{0}/", _configFileProvider.Port);
            _localHttpsUrl = String.Format("https://localhost:{0}/", _configFileProvider.SslPort);
            _wildcardHttpsUrl = String.Format("https://*:{0}/", _configFileProvider.SslPort);

            Url = _wildcardUrl;
            HttpsUrl = _wildcardHttpsUrl;
        }

        public void ConfigureUrl()
        {
            if (!_runtimeInfo.IsAdmin)
            {
                if (!IsRegistered(_wildcardUrl)) Url = _localUrl;
                if (!IsRegistered(_wildcardHttpsUrl)) HttpsUrl = _localHttpsUrl;                
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

            RegisterUrl(Url);
            RegisterUrl(HttpsUrl);
        }
        
        private bool IsRegistered(string urlAcl)
        {
            var arguments = String.Format("http show urlacl {0}", urlAcl);
            var output = _netshProvider.Run(arguments);

            if (output == null || !output.Standard.Any()) return false;

            return output.Standard.Any(line => line.Contains(urlAcl));
        }

        private void RegisterUrl(string urlAcl)
        {
            var arguments = String.Format("http add urlacl {0} sddl=D:(A;;GX;;;S-1-1-0)", urlAcl);
            _netshProvider.Run(arguments);
        }
    }
}