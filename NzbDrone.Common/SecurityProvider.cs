using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using NLog;
#if __MonoCS__
#else
using NetFwTypeLib;
#endif

namespace NzbDrone.Common
{
    public interface ISecurityProvider
    {
        void MakeAccessible();
        bool IsCurrentUserAdmin();
        bool IsNzbDronePortOpen();
        bool IsNzbDroneUrlRegistered();
    }

	public class SecurityProvider : ISecurityProvider
	{
		private readonly ConfigFileProvider _configFileProvider;
		private readonly EnvironmentProvider _environmentProvider;
		private readonly ProcessProvider _processProvider;
	    private readonly Logger _logger;

	    public SecurityProvider(ConfigFileProvider configFileProvider, EnvironmentProvider environmentProvider,
                                    ProcessProvider processProvider, Logger logger)
		{
			_configFileProvider = configFileProvider;
			_environmentProvider = environmentProvider;
			_processProvider = processProvider;
		    _logger = logger;
		}

		public void MakeAccessible()
		{
			if (!IsCurrentUserAdmin ())
            {
				_logger.Trace ("User is not an admin, skipping.");
				return;
			}

			int port = 0;

			if (IsFirewallEnabled ())
            {
				if (IsNzbDronePortOpen ())
                {
					_logger.Trace ("NzbDrone port is already open, skipping.");
					return;
				}

				//Close any old ports
				port = CloseFirewallPort ();

				//Open the new port
				OpenFirewallPort (_configFileProvider.Port);
			}

			//Skip Url Register if not Vista or 7
			if (_environmentProvider.GetOsVersion ().Major < 6)
				return;

			//Unregister Url (if port != 0)
			if (port != 0)
				UnregisterUrl(port);

			//Register Url
			RegisterUrl(_configFileProvider.Port);
		}

		public bool IsCurrentUserAdmin()
		{
			try {
				var principal = new WindowsPrincipal (WindowsIdentity.GetCurrent ());
				return principal.IsInRole (WindowsBuiltInRole.Administrator);
			} catch (Exception ex) {
				_logger.WarnException ("Error checking if the current user is an administrator.", ex);
				return false;
			}
		}

		public bool IsNzbDronePortOpen()
		{
#if __MonoCS__
#else

			try {
				var netFwMgrType = Type.GetTypeFromProgID ("HNetCfg.FwMgr", false);

				var mgr = (INetFwMgr)Activator.CreateInstance (netFwMgrType);

				if (!mgr.LocalPolicy.CurrentProfile.FirewallEnabled)
					return false;

				var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

				foreach (INetFwOpenPort p in ports) {
					if (p.Port == _configFileProvider.Port)
						return true;
				}
			}
            catch (Exception ex) {
				_logger.WarnException ("Failed to check for open port in firewall", ex);
			}
#endif
			return false;
		}

        public bool IsNzbDroneUrlRegistered()
        {
            return CheckIfUrlIsRegisteredUrl(_configFileProvider.Port);
        }

		private void OpenFirewallPort(int portNumber)
		{
#if __MonoCS__
			return true;
#else
			try {
				var type = Type.GetTypeFromProgID ("HNetCfg.FWOpenPort", false);
				var port = Activator.CreateInstance (type) as INetFwOpenPort;

				port.Port = portNumber;
				port.Name = "NzbDrone";
				port.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
				port.Enabled = true;

				var netFwMgrType = Type.GetTypeFromProgID ("HNetCfg.FwMgr", false);
				var mgr = (INetFwMgr)Activator.CreateInstance (netFwMgrType);
				var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

				ports.Add (port);
			}
            catch (Exception ex) {
				_logger.WarnException ("Failed to open port in firewall for NzbDrone " + portNumber, ex);
			}
#endif
		}

		private int CloseFirewallPort()
		{

#if __MonoCS__
#else

			try {
				var netFwMgrType = Type.GetTypeFromProgID ("HNetCfg.FwMgr", false);
				var mgr = (INetFwMgr)Activator.CreateInstance (netFwMgrType);
				var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

				var portNumber = 8989;

				foreach (INetFwOpenPort p in ports) {
					if (p.Name == "NzbDrone") {
						portNumber = p.Port;
						break;
					}
				}

				if (portNumber != _configFileProvider.Port)
                {
					ports.Remove (portNumber, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
					return portNumber;
				}
			}
            catch (Exception ex)
            {
				_logger.WarnException ("Failed to close port in firewall for NzbDrone", ex);
			}
#endif
			return 0;
		}

		private bool IsFirewallEnabled()
		{
#if __MonoCS__
			return true;
#else

			try {
				var netFwMgrType = Type.GetTypeFromProgID ("HNetCfg.FwMgr", false);
				var mgr = (INetFwMgr)Activator.CreateInstance (netFwMgrType);
				return mgr.LocalPolicy.CurrentProfile.FirewallEnabled;
			}
            catch (Exception ex)
            {
				_logger.WarnException ("Failed to check if the firewall is enabled", ex);
				return false;
			}
#endif
		}

		private void RegisterUrl(int portNumber)
		{
		    var arguments = String.Format("http add urlacl http://+:{0}/ user=EVERYONE", portNumber);
		    RunNetsh(arguments);
		}

		private void UnregisterUrl(int portNumber)
		{
		    var arguments = String.Format("http delete urlacl http://+:{0}/", portNumber);
            RunNetsh(arguments);
		}

        private bool CheckIfUrlIsRegisteredUrl(int portNumber)
        {
            var url = String.Format("http://+:{0}/", portNumber);
            var arguments = String.Format("http show urlacl url=\"{0}\"", url);
            var output = RunNetsh(arguments);

            if(String.IsNullOrWhiteSpace(output))
            {
                _logger.Error("netsh output is invalid for arguments: {0}", arguments);
            }

            if(!output.Contains(url))
            {
                _logger.Trace("Url has not already been registered");
                return false;
            }

            _logger.Trace("Url has already been registered!");
            return true;
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
