using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ExternalNotification
{
    public class Xbmc : ExternalNotificationBase
    {
        private readonly IConfigService _configService;
        private readonly XbmcProvider _xbmcProvider;

        public Xbmc(IConfigService configService, IExternalNotificationRepository repository, XbmcProvider xbmcProvider, Logger logger)
            : base(repository, logger)
        {
            _configService = configService;
            _xbmcProvider = xbmcProvider;
        }

        public override string Name
        {
            get { return "XBMC"; }
        }

        protected override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";

            _xbmcProvider.Notify(header, message);
        }

        protected override void OnDownload(string message, Series series)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            _xbmcProvider.Notify(header, message);
            UpdateAndClean(series);
        }

        protected override void AfterRename(Series series)
        {
            UpdateAndClean(series);
        }

        private void UpdateAndClean(Series series)
        {
            if (_configService.XbmcUpdateLibrary)
            {
                _xbmcProvider.Update(series);
            }

            if (_configService.XbmcCleanLibrary)
            {
                _xbmcProvider.Clean();
            }
        }
    }
}
