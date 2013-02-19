using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Lifecycle
{
    public class AppRestartJob : IJob
    {
        private readonly HostController _hostController;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AppRestartJob(HostController hostController)
        {
            _hostController = hostController;
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

            _hostController.StopServer();
        }
    }
}