using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Host.AccessControl
{
    public class RemoteAccessAdapter : IRemoteAccessAdapter
    {
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IFirewallAdapter _firewallAdapter;

        public RemoteAccessAdapter(IRuntimeInfo runtimeInfo,
                                   IFirewallAdapter firewallAdapter)
        {
            _runtimeInfo = runtimeInfo;
            _firewallAdapter = firewallAdapter;
        }

        public void MakeAccessible(bool passive)
        {
            if (OsInfo.IsWindows)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _firewallAdapter.MakeAccessible();
                }
                else if (!passive)
                {
                    throw new RemoteAccessException("Failed to register URLs for Sonarr. Sonarr will not be accessible remotely");
                }
            }
        }
    }
}
