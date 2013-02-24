using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Growl : ExternalNotificationBase
    {
        private readonly GrowlProvider _growlProvider;

        public Growl(IConfigService configService, GrowlProvider growlProvider)
            : base(configService)
        {
            _growlProvider = growlProvider;
        }

        public override string Name
        {
            get { return "Growl"; }
        }

        public override void OnGrab(string message)
        {
            try
            {
                if(_configService.GrowlNotifyOnGrab)
                {
                    _logger.Trace("Sending Notification to Growl");
                    const string title = "Episode Grabbed";

                    var growlHost = _configService.GrowlHost.Split(':');
                    var host = growlHost[0];
                    var port = Convert.ToInt32(growlHost[1]);

                    _growlProvider.SendNotification(title, message, "GRAB", host, port, _configService.GrowlPassword);
                }
            }

            catch (Exception ex)
            {
                _logger.WarnException(ex.Message, ex);
            }
        }

        public override void OnDownload(string message, Series series)
        {
            try
            {
                if (_configService.GrowlNotifyOnDownload)
                {
                    _logger.Trace("Sending Notification to Growl");
                    const string title = "Episode Downloaded";
                    
                    var growlHost = _configService.GrowlHost.Split(':');
                    var host = growlHost[0];
                    var port = Convert.ToInt32(growlHost[1]);

                    _growlProvider.SendNotification(title, message, "DOWNLOAD", host, port, _configService.GrowlPassword);
                }
            }

            catch (Exception ex)
            {
                _logger.WarnException(ex.Message, ex);
            }
        }


        public override void AfterRename(string message, Series series)
        {

        }
    }
}
