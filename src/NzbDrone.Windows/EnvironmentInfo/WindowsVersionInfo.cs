using System;
using Microsoft.Win32;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Windows.EnvironmentInfo
{
    public class WindowsVersionInfo : IOsVersionAdapter
    {
        private readonly Logger _logger;
        public bool Enabled => OsInfo.IsWindows;

        public WindowsVersionInfo(Logger logger)
        {
            _logger = logger;
        }

        public OsVersionModel Read()
        {
            var windowsServer = IsServer();
            var osName = windowsServer ? "Windows Server" : "Windows";
            return new OsVersionModel(osName, Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString);
        }

        private bool IsServer()
        {
            try
            {
                const string subkey = @"Software\Microsoft\Windows NT\CurrentVersion";
                var openSubKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey);
                if (openSubKey != null)
                {
                    var productName = openSubKey.GetValue("ProductName").ToString();

                    if (productName.ToLower().Contains("server"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't detect if running Windows Server");
            }

            return false;
        }
    }
}
