using System;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RenameEpisodeJob : IJob
    {
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RenameEpisodeJob(DiskProvider diskProvider, EpisodeProvider episodeProvider,
                                MediaFileProvider mediaFileProvider, SeriesProvider seriesProvider)
        {
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _seriesProvider = seriesProvider;
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
            _mediaFileProvider.RenameEpisodeFile(targetId, notification);
        }
    }
}