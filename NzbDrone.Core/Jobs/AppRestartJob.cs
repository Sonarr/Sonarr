using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs
{
    public class AppRestartJob : IJob
    {
        private readonly EnvironmentProvider _environmentProvider;
        private readonly ProcessProvider _processProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly IISProvider _iisProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AppRestartJob(EnvironmentProvider environmentProvider, ProcessProvider processProvider,
                                ServiceProvider serviceProvider, IISProvider iisProvider)
        {
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _serviceProvider = serviceProvider;
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