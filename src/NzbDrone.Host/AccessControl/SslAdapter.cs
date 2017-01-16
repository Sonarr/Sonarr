using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface ISslAdapter
    {
        void Register();
    }

    public class SslAdapter : ISslAdapter
    {
        private const string APP_ID = "C2172AF4-F9A6-4D91-BAEE-C2E4EE680613";
        private static readonly Regex CertificateHashRegex = new Regex(@"^\s+(?:Certificate Hash\s+:\s+)(?<hash>\w+)", RegexOptions.Compiled);

        private readonly INetshProvider _netshProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public SslAdapter(INetshProvider netshProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _netshProvider = netshProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void Register()
        {
            if (!_configFileProvider.EnableSsl) return;
            if (IsRegistered()) return;

            if (string.IsNullOrWhiteSpace(_configFileProvider.SslCertHash))
            {
                _logger.Warn("Unable to enable SSL, SSL Cert Hash is required");
                return;
            }

            var arguments = string.Format("http add sslcert ipport=0.0.0.0:{0} certhash={1} appid={{{2}}}",
                                            _configFileProvider.SslPort,
                                            _configFileProvider.SslCertHash,
                                            APP_ID);

            //TODO: Validate that the cert was added properly, invisible spaces FTL
            _netshProvider.Run(arguments);
        }

        private bool IsRegistered()
        {
            var ipPort = "0.0.0.0:" + _configFileProvider.SslPort;
            var arguments = string.Format("http show sslcert ipport={0}", ipPort);

            var output = _netshProvider.Run(arguments);

            if (output == null || !output.Standard.Any()) return false;

            var hashLine = output.Standard.SingleOrDefault(line => CertificateHashRegex.IsMatch(line.Content));

            if (hashLine != null)
            {
                var match = CertificateHashRegex.Match(hashLine.Content);

                if (match.Success)
                {
                    if (match.Groups["hash"].Value != _configFileProvider.SslCertHash)
                    {
                        Unregister();

                        return false;
                    }
                }
            }

            return output.Standard.Any(line => line.Content.Contains(ipPort));
        }

        private void Unregister()
        {
            var ipPort = "0.0.0.0:" + _configFileProvider.SslPort;
            var arguments = string.Format("http delete sslcert ipport={0}", ipPort);

            _netshProvider.Run(arguments);
        }
    }
}
