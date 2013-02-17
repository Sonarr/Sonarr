using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs
{
    public class AppRestartJob : IJob
    {
        private readonly IISProvider _iisProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AppRestartJob(IISProvider iisProvider)
        {
            _iisProvider = iisProvider;
        }

        public string Name
        {
            get { return "Restart NzbDrone"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            notification.CurrentMessage = "Restarting NzbDrone";
            logger.Info("Restarting NzbDrone");

            _iisProvider.StopServer();
        }
    }
}