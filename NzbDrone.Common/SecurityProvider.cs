using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using NLog;
using NetFwTypeLib;
using Ninject;

namespace NzbDrone.Common
{
    public class SecurityProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConfigFileProvider _configFileProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly ProcessProvider _processProvider;

        [Inject]
        public SecurityProvider(ConfigFileProvider configFileProvider, EnviromentProvider enviromentProvider,
                                    ProcessProvider processProvider)
        {
            _configFileProvider = configFileProvider;
            _enviromentProvider = enviromentProvider;
            _processProvider = processProvider;
        }

        public SecurityProvider()
        {
        }

        public virtual void MakeAccessible()
        {
            if (!IsCurrentUserAdmin())
            {
                Logger.Trace("User is not an admin, skipping.");
                return;
            }

            int port = 0;

            if (IsFirewallEnabled())
            {
                if(IsNzbDronePortOpen())
                {
                    Logger.Trace("NzbDrone port is already open, skipping.");
                    return;
                }

                //Close any old ports
                port = CloseFirewallPort();

                //Open the new port
                OpenFirewallPort(_configFileProvider.Port);
            }

            //Skip Url Register if not Vista or 7
            if (_enviromentProvider.GetOsVersion().Major < 6)
                return;

            //Unregister Url (if port != 0)
            if (port != 0)
                UnregisterUrl(port);

            //Register Url
            RegisterUrl(_configFileProvider.Port);
        }

        public virtual bool IsCurrentUserAdmin()
        {
            try
            {
                var currentIdentity = WindowsIdentity.GetCurrent();

                var principal = new WindowsPrincipal(currentIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch(Exception ex)
            {
                Logger.WarnException("Error checking if the current user is an administrator.", ex);
                return false;
            }
        }

        public virtual bool IsNzbDronePortOpen()
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);

                if (!mgr.LocalPolicy.CurrentProfile.FirewallEnabled)
                    return false;

                var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

                foreach (INetFwOpenPort p in ports)
                {
                    if (p.Port == _configFileProvider.Port)
                        return true;
                }
            }
            catch(Exception ex)
            {
                Logger.WarnException("Failed to check for open port in firewall", ex);
            }
            return false;
        }

        private bool OpenFirewallPort(int portNumber)
        {
            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FWOpenPort", false);
                var port = Activator.CreateInstance(type) as INetFwOpenPort;

                port.Port = portNumber;
                port.Name = "NzbDrone";
                port.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                port.Enabled = true;

                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

                ports.Add(port);
                return true;
            }
            catch(Exception ex)
            {
                Logger.WarnException("Failed to open port in firewall for NzbDrone " + portNumber, ex);
                return false;
            }
        }

        private int CloseFirewallPort()
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

                var portNumber = 8989;

                foreach (INetFwOpenPort p in ports)
                {
                    if (p.Name == "NzbDrone")
                    {
                        portNumber = p.Port;
                        break;
                    }
                }

                if (portNumber != _configFileProvider.Port)
                {
                    ports.Remove(portNumber, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
                    return portNumber;
                }
            }
            catch(Exception ex)
            {
                Logger.WarnException("Failed to close port in firewall for NzbDrone", ex);
            }

            return 0;
        }

        private bool IsFirewallEnabled()
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                return mgr.LocalPolicy.CurrentProfile.FirewallEnabled;
            }

            catch(Exception ex)
            {
                Logger.WarnException("Failed to check if the firewall is enabled", ex);
                return false;
            }
        }

        private bool RegisterUrl(int portNumber)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "netsh.exe",
                    Arguments = string.Format("http add urlacl http://*:{0}/ user=EVERYONE", portNumber)
                };

                var process = _processProvider.Start(startInfo);
                process.WaitForExit(5000);
                return true;
            }

            catch(Exception ex)
            {
                Logger.WarnException("Error registering URL", ex);
            }

            return false;
        }

        private bool UnregisterUrl(int portNumber)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "netsh.exe",
                    Arguments = string.Format("http delete urlacl http://*:{0}/", portNumber)
                };

                var process = _processProvider.Start(startInfo);
                process.WaitForExit(5000);
                return true;
            }

            catch (Exception ex)
            {
                Logger.WarnException("Error registering URL", ex);
            }

            return false;
        }
    }
}
