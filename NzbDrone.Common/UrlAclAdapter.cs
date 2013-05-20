using System;
using System.Diagnostics;
using NLog;

namespace NzbDrone.Common
{
    public interface IUrlAclAdapter
    {
        void RefreshRegistration();
    }

    public class UrlAclAdapter : IUrlAclAdapter
    {
        private readonly IProcessProvider _processProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly Logger _logger;

        public UrlAclAdapter(IProcessProvider processProvider, IConfigFileProvider configFileProvider, IEnvironmentProvider environmentProvider, Logger logger)
        {
            _processProvider = processProvider;
            _configFileProvider = configFileProvider;
            _environmentProvider = environmentProvider;
            _logger = logger;
        }

        public void RefreshRegistration()
        {
            if (_environmentProvider.GetOsVersion().Major < 6)
                return;

            RegisterUrl(_configFileProvider.Port);
        }


        private void RegisterUrl(int portNumber)
        {
            var arguments = String.Format("http add urlacl http://*:{0}/ user=EVERYONE", portNumber);
            RunNetsh(arguments);
        }

        private string RunNetsh(string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = "netsh.exe",
                        Arguments = arguments
                    };

                var process = _processProvider.Start(startInfo);
                process.WaitForExit(5000);
                return process.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.WarnException("Error executing netsh with arguments: " + arguments, ex);
            }

            return null;
        }
    }
}