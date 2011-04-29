using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public abstract class ExternalNotificationProviderBase
    {
        protected readonly Logger _logger;
        protected readonly ConfigProvider _configProvider;
        protected readonly ExternalNotificationProvider _externalNotificationProvider;

        public ExternalNotificationProviderBase(ConfigProvider configProvider, ExternalNotificationProvider externalNotificationProvider)
        {
            _configProvider = configProvider;
            _externalNotificationProvider = externalNotificationProvider;
            _logger = LogManager.GetLogger(GetType().ToString());
        }

        /// <summary>
        ///   Gets the name for the notification provider
        /// </summary>
        public abstract string Name { get; }

        public ExternalNotificationSetting Settings
        {
            get
            {
                return _externalNotificationProvider.GetSettings(GetType());
            }
        }

        public virtual void Notify(ExternalNotificationType type, string message, int seriesId = 0)
        {
            if (type == ExternalNotificationType.Grab)
                OnGrab(message);

            else if (type == ExternalNotificationType.Download)
                OnDownload(message, seriesId);

            else if (type == ExternalNotificationType.Rename)
                OnRename(message, seriesId);
        }

        /// <summary>
        ///   Performs the on grab action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        public abstract void OnGrab(string message);

        /// <summary>
        ///   Performs the on download action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        /// <param name = "seriesId">The Series ID for the new download</param>
        public abstract void OnDownload(string message, int seriesId);

        /// <summary>
        ///   Performs the on rename action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        /// <param name = "seriesId">The Series ID for the new download</param>
        public abstract void OnRename(string message, int seriesId);
    }
}
