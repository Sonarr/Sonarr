using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RenameEpisodeJob : IJob
    {
        private readonly DiskScanProvider _diskScanProvider;
        private readonly MediaFileProvider _mediaFileProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameEpisodeJob(DiskScanProvider diskScanProvider, MediaFileProvider mediaFileProvider)
        {
            _diskScanProvider = diskScanProvider;
            _mediaFileProvider = mediaFileProvider;
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
            var episode = _mediaFileProvider.GetEpisodeFile(targetId);
            _diskScanProvider.RenameEpisodeFile(episode);
        }
    }
}