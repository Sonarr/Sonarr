using System;
using NetFwTypeLib;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IFirewallAdapter
    {
        void MakeAccessible();
    }

    public class FirewallAdapter : IFirewallAdapter
    {
        private const NET_FW_PROFILE_TYPE_ FIREWALL_PROFILE = NET_FW_PROFILE_TYPE_.NET_FW_PROFILE_STANDARD;
        
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;
        
        public FirewallAdapter(IConfigFileProvider configFileProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void MakeAccessible()
        {
            if (IsFirewallEnabled())
            {
                if (!IsNzbDronePortOpen(_configFileProvider.Port))
                {
                    _logger.Trace("Opening Port for NzbDrone: {0}", _configFileProvider.Port);
                    OpenFirewallPort(_configFileProvider.Port);
                }

                if (_configFileProvider.EnableSsl && !IsNzbDronePortOpen(_configFileProvider.SslPort))
                {
                    _logger.Trace("Opening SSL Port for NzbDrone: {0}", _configFileProvider.SslPort);
                    OpenFirewallPort(_configFileProvider.SslPort);
                }
            }
        }

        private bool IsNzbDronePortOpen(int port)
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);

                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                var ports = mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).GloballyOpenPorts;

                foreach (INetFwOpenPort p in ports)
                {
                    if (p.Port == port)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to check for open port in firewall", ex);
            }
            return false;
        }

        private void OpenFirewallPort(int portNumber)
        {
            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FWOpenPort", false);
                var port = (INetFwOpenPort)Activator.CreateInstance(type);

                port.Port = portNumber;
                port.Name = "NzbDrone";
                port.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                port.Enabled = true;

                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);

                //Open the port for the standard profile, should help when the user has multiple network adapters
                mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).GloballyOpenPorts.Add(port);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to open port in firewall for NzbDrone " + portNumber, ex);
            }
        }

        private bool IsFirewallEnabled()
        {
            if (OsInfo.IsMono) return false;

            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                return mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).FirewallEnabled;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to check if the firewall is enabled", ex);
                return false;
            }
        }
    }
}
