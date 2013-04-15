using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public interface IProvideDownloadClient
    {
        IDownloadClient GetDownloadClient();
    }

    public class DownloadClientProvider : IProvideDownloadClient
    {

        private readonly SabnzbdClient _sabnzbdClient;
        private readonly IConfigService _configService;
        private readonly BlackholeProvider _blackholeProvider;
        private readonly PneumaticClient _pneumaticClient;
        private readonly NzbgetClient _nzbgetClient;


        public DownloadClientProvider(SabnzbdClient sabnzbdClient, IConfigService configService,
                                      BlackholeProvider blackholeProvider,
                                      PneumaticClient pneumaticClient,
                                      NzbgetClient nzbgetClient)
        {
            _sabnzbdClient = sabnzbdClient;
            _configService = configService;
            _blackholeProvider = blackholeProvider;
            _pneumaticClient = pneumaticClient;
            _nzbgetClient = nzbgetClient;
        }

        public IDownloadClient GetDownloadClient()
        {
            switch (_configService.DownloadClient)
            {
                case DownloadClientType.Blackhole:
                    return _blackholeProvider;

                case DownloadClientType.Pneumatic:
                    return _pneumaticClient;

                case DownloadClientType.Nzbget:
                    return _nzbgetClient;

                default:
                    return _sabnzbdClient;
            }
        }
    }
}