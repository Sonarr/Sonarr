using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationBase<XbmcSettings>
    {
        private readonly IXbmcService _xbmcService;

        public Xbmc(IXbmcService xbmcService)
        {
            _xbmcService = xbmcService;
        }

        public override string Link
        {
            get { return "http://xbmc.org/"; }
        }

        public override void OnGrab(string message)
        {
            const string header = "NzbDrone [TV] - Grabbed";

            if (Settings.Notify)
            {
                _xbmcService.Notify(Settings, header, message);
            }
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "NzbDrone [TV] - Downloaded";

            if (Settings.Notify)
            {
                _xbmcService.Notify(Settings, header, message.Message);
            }

            UpdateAndClean(message.Series, message.OldFiles.Any());
        }

        public override void AfterRename(Series series)
        {
            UpdateAndClean(series);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_xbmcService.Test(Settings));

            return new ValidationResult(failures);
        }

        private void UpdateAndClean(Series series, bool clean = true)
        {
            if (Settings.UpdateLibrary)
            {
                _xbmcService.Update(Settings, series);
            }

            if (clean && Settings.CleanLibrary)
            {
                _xbmcService.Clean(Settings);
            }
        }
    }
}
