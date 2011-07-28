using System;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public abstract class ExternalNotificationBase
    {
        protected readonly Logger _logger;
        protected readonly ConfigProvider _configProvider;

        protected ExternalNotificationBase(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
            _logger = LogManager.GetLogger(GetType().ToString());
        }

        /// <summary>
        ///   Gets the name for the notification provider
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   Performs the on grab action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        public abstract void OnGrab(string message);

        /// <summary>
        ///   Performs the on download action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        /// <param name = "series">The Series for the new download</param>
        public abstract void OnDownload(string message, Series series);

        /// <summary>
        ///   Performs the on rename action
        /// </summary>
        /// <param name = "message">The message to send to the receiver</param>
        /// <param name = "series">The Series for the new download</param>
        public abstract void OnRename(string message, Series series);
    }
}
