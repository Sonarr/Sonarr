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

        private readonly SabProvider _sabProvider;
        private readonly IConfigService _configService;
        private readonly BlackholeProvider _blackholeProvider;
        private readonly PneumaticProvider _pneumaticProvider;
        private readonly NzbgetProvider _nzbgetProvider;


        public DownloadClientProvider(SabProvider sabProvider, IConfigService configService,
                                      BlackholeProvider blackholeProvider,
                                      PneumaticProvider pneumaticProvider,
                                      NzbgetProvider nzbgetProvider)
        {
            _sabProvider = sabProvider;
            _configService = configService;
            _blackholeProvider = blackholeProvider;
            _pneumaticProvider = pneumaticProvider;
            _nzbgetProvider = nzbgetProvider;
        }

        public IDownloadClient GetDownloadClient()
        {
            switch (_configService.DownloadClient)
            {
                case DownloadClientType.Blackhole:
                    return _blackholeProvider;

                case DownloadClientType.Pneumatic:
                    return _pneumaticProvider;

                case DownloadClientType.Nzbget:
                    return _nzbgetProvider;

                default:
                    return _sabProvider;
            }
        }
    }
}