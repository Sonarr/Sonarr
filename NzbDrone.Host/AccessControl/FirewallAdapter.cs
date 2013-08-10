using System;
using NetFwTypeLib;
using NLog;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host.AccessControl
{
    public interface IFirewallAdapter
    {
        void MakeAccessible();
    }

    public class FirewallAdapter : IFirewallAdapter
    {
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
                if (IsNzbDronePortOpen())
                {
                    _logger.Trace("NzbDrone port is already open, skipping.");
                    return;
                }

                CloseFirewallPort();

                //Open the new port
                OpenFirewallPort(_configFileProvider.Port);
            }
        }


        private bool IsNzbDronePortOpen()
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
                var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

                ports.Add(port);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to open port in firewall for NzbDrone " + portNumber, ex);
            }
        }

        private void CloseFirewallPort()
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
                }
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to close port in firewall for NzbDrone", ex);
            }
        }

        private bool IsFirewallEnabled()
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                return mgr.LocalPolicy.CurrentProfile.FirewallEnabled;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to check if the firewall is enabled", ex);
                return false;
            }
        }


    }
}
