using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RenameEpisodeJob : IJob
    {
        private readonly RenameProvider _renameProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RenameEpisodeJob(RenameProvider renameProvider)
        {
            _renameProvider = renameProvider;
        }

        public string Name
        {
            get { return "Rename Episode"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            _renameProvider.RenameEpisodeFile(targetId, notification);
        }
    }
}