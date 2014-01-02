using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IUrlAclAdapter
    {
        void ConfigureUrl();
        List<String> Urls { get; }
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private readonly INetshProvider _netshProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public List<String> Urls { get; private set; }

        public UrlAclAdapter(INetshProvider netshProvider,
                             IConfigFileProvider configFileProvider,
                             IRuntimeInfo runtimeInfo,
                             Logger logger)
        {
            _netshProvider = netshProvider;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _logger = logger;

            Urls = new List<String>();
        }

        public void ConfigureUrl()
        {
            var localHttpUrls = BuildUrls("http", "localhost", _configFileProvider.Port);
            var wildcardHttpUrls = BuildUrls("http", "*", _configFileProvider.Port);

            var localHttpsUrls = BuildUrls("https", "localhost", _configFileProvider.SslPort);
            var wildcardHttpsUrls = BuildUrls("https", "*", _configFileProvider.SslPort);

            if (!_runtimeInfo.IsAdmin)
            {
                var httpUrls = wildcardHttpUrls.All(IsRegistered) ? wildcardHttpUrls : localHttpUrls;
                var httpsUrls = wildcardHttpsUrls.All(IsRegistered) ? wildcardHttpsUrls : localHttpsUrls;

                Urls.AddRange(httpUrls);
                Urls.AddRange(httpsUrls);             
            }

            else
            {
                Urls.AddRange(wildcardHttpUrls);
                Urls.AddRange(wildcardHttpsUrls);   

                RefreshRegistration();
            }
        }

        private void RefreshRegistration()
        {
            if (OsInfo.Version.Major < 6)
                return;

            Urls.ForEach(RegisterUrl);
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

        private string BuildUrl(string protocol, string url, int port, string urlBase)
        {
            var result = protocol + "://" + url + ":" + port;
            result += String.IsNullOrEmpty(urlBase) ? "/" : urlBase + "/";

            return result;
        }

        private List<String> BuildUrls(string protocol, string url, int port)
        {
            var urls = new List<String>();
            var urlBase = _configFileProvider.UrlBase;

            if (!String.IsNullOrEmpty(urlBase))
            {
                urls.Add(BuildUrl(protocol, url, port, urlBase));
            }

            urls.Add(BuildUrl(protocol, url, port, ""));

            return urls;
        }
    }
}