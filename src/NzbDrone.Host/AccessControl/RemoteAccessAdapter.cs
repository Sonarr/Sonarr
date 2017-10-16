using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Host.AccessControl
{
    public interface IRemoteAccessAdapter
    {
        void MakeAccessible(bool passive);
    }

    public class RemoteAccessAdapter : IRemoteAccessAdapter
    {
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IUrlAclAdapter _urlAclAdapter;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly ISslAdapter _sslAdapter;

        public RemoteAccessAdapter(IRuntimeInfo runtimeInfo,
                                   IUrlAclAdapter urlAclAdapter,
                                   IFirewallAdapter firewallAdapter,
                                   ISslAdapter sslAdapter)
        {
            _runtimeInfo = runtimeInfo;
            _urlAclAdapter = urlAclAdapter;
            _firewallAdapter = firewallAdapter;
            _sslAdapter = sslAdapter;
        }

        public void MakeAccessible(bool passive)
        {
            if (OsInfo.IsWindows)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _firewallAdapter.MakeAccessible();
                    _sslAdapter.Register();
                }
                else if (!passive)
                {
                    throw new RemoteAccessException("Failed to register URLs for Sonarr. Sonarr will not be accessible remotely");
                }
            }

            _urlAclAdapter.ConfigureUrls();
        }
    }
}
