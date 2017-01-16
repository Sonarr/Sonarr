using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IUrlAclAdapter
    {
        void ConfigureUrls();
        List<string> Urls { get; }
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private readonly INetshProvider _netshProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IOsInfo _osInfo;
        private readonly Logger _logger;

        public List<string> Urls
        {
            get
            {
                return InternalUrls.Select(c => c.Url).ToList();
            }
        }

        private List<UrlAcl> InternalUrls { get; }
        private List<UrlAcl> RegisteredUrls { get; } 

        private static readonly Regex UrlAclRegex = new Regex(@"(?<scheme>https?)\:\/\/(?<address>.+?)\:(?<port>\d+)/(?<urlbase>.+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public UrlAclAdapter(INetshProvider netshProvider,
                             IConfigFileProvider configFileProvider,
                             IRuntimeInfo runtimeInfo,
                             IOsInfo osInfo,
                             Logger logger)
        {
            _netshProvider = netshProvider;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _osInfo = osInfo;
            _logger = logger;

            InternalUrls = new List<UrlAcl>();
            RegisteredUrls = GetRegisteredUrls();
        }

        public void ConfigureUrls()
        {
            var localHostHttpUrls = BuildUrlAcls("http", "localhost", _configFileProvider.Port);
            var interfaceHttpUrls = BuildUrlAcls("http", _configFileProvider.BindAddress, _configFileProvider.Port);

            var localHostHttpsUrls = BuildUrlAcls("https", "localhost", _configFileProvider.SslPort);
            var interfaceHttpsUrls = BuildUrlAcls("https", _configFileProvider.BindAddress, _configFileProvider.SslPort);

            if (!_configFileProvider.EnableSsl)
            {
                localHostHttpsUrls.Clear();
                interfaceHttpsUrls.Clear();
            }

            if (OsInfo.IsWindows && !_runtimeInfo.IsAdmin)
            {
                var httpUrls = interfaceHttpUrls.All(IsRegistered) ? interfaceHttpUrls : localHostHttpUrls;
                var httpsUrls = interfaceHttpsUrls.All(IsRegistered) ? interfaceHttpsUrls : localHostHttpsUrls;

                InternalUrls.AddRange(httpUrls);
                InternalUrls.AddRange(httpsUrls);

                if (_configFileProvider.BindAddress != "*")
                {
                    if (httpUrls.None(c => c.Address.Equals("localhost")))
                    {
                        InternalUrls.AddRange(localHostHttpUrls);
                    }

                    if (httpsUrls.None(c => c.Address.Equals("localhost")))
                    {
                        InternalUrls.AddRange(localHostHttpsUrls);
                    }
                }
            }
            else
            {
                InternalUrls.AddRange(interfaceHttpUrls);
                InternalUrls.AddRange(interfaceHttpsUrls);

                //Register localhost URLs so the IP Address doesn't need to be used from the local system
                if (_configFileProvider.BindAddress != "*")
                {
                    InternalUrls.AddRange(localHostHttpUrls);
                    InternalUrls.AddRange(localHostHttpsUrls);
                }

                if (OsInfo.IsWindows)
                {
                    RefreshRegistration();
                }
            }
        }

        private void RefreshRegistration()
        {
            var osVersion = new Version(_osInfo.Version);
            if (osVersion.Major < 6) return;

            foreach (var urlAcl in InternalUrls)
            {
                if (IsRegistered(urlAcl) || urlAcl.Address.Equals("localhost")) continue;

                RemoveSimilar(urlAcl);
                RegisterUrl(urlAcl);
            }
        }
        
        private bool IsRegistered(UrlAcl urlAcl)
        {
            return RegisteredUrls.Any(c => c.Scheme == urlAcl.Scheme &&
                                      c.Address == urlAcl.Address &&
                                      c.Port == urlAcl.Port &&
                                      c.UrlBase == urlAcl.UrlBase);
        }

        private List<UrlAcl> GetRegisteredUrls()
        {
            if (OsInfo.IsNotWindows)
            {
                return new List<UrlAcl>();
            }

            var arguments = string.Format("http show urlacl");
            var output = _netshProvider.Run(arguments);

            if (output == null || !output.Standard.Any()) return new List<UrlAcl>();

            return output.Standard.Select(line =>
            {
                var match = UrlAclRegex.Match(line.Content);

                if (match.Success)
                {
                    return new UrlAcl
                           {
                               Scheme = match.Groups["scheme"].Value,
                               Address = match.Groups["address"].Value,
                               Port = Convert.ToInt32(match.Groups["port"].Value),
                               UrlBase = match.Groups["urlbase"].Value.Trim()
                           };
                }

                return null;

            }).Where(r => r != null).ToList();
        }

        private void RegisterUrl(UrlAcl urlAcl)
        {
            var arguments = string.Format("http add urlacl {0} sddl=D:(A;;GX;;;S-1-1-0)", urlAcl.Url);
            _netshProvider.Run(arguments);
        }

        private void RemoveSimilar(UrlAcl urlAcl)
        {
            var similar = RegisteredUrls.Where(c => c.Scheme == urlAcl.Scheme &&
                                                    InternalUrls.None(x => x.Address == c.Address) &&
                                                    c.Port == urlAcl.Port &&
                                                    c.UrlBase == urlAcl.UrlBase);

            foreach (var s in similar)
            {
                UnregisterUrl(s);
            }
        }

        private void UnregisterUrl(UrlAcl urlAcl)
        {
            _logger.Trace("Removing URL ACL {0}", urlAcl.Url);

            var arguments = string.Format("http delete urlacl {0}", urlAcl.Url);
            _netshProvider.Run(arguments);
        }

        private List<UrlAcl> BuildUrlAcls(string scheme, string address, int port)
        {
            var urlAcls = new List<UrlAcl>();
            var urlBase = _configFileProvider.UrlBase;

            if (urlBase.IsNotNullOrWhiteSpace())
            {
                urlAcls.Add(new UrlAcl
                         {
                             Scheme = scheme,
                             Address = address,
                             Port = port,
                             UrlBase = urlBase.Trim('/') + "/"
                         });
            }

            urlAcls.Add(new UrlAcl
            {
                Scheme = scheme,
                Address = address,
                Port = port,
                UrlBase = string.Empty
            });

            return urlAcls;
        }
    }
}
