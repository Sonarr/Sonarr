using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class AutoIgnoreJob : IJob
    {
        private readonly ConfigProvider _configProvider;
        private readonly EpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public AutoIgnoreJob(ConfigProvider configProvider, EpisodeProvider episodeProvider)
        {
            _configProvider = configProvider;
            _episodeProvider = episodeProvider;
        }

        public AutoIgnoreJob()
        {
        }

        public string Name
        {
            get { return "Auto Ignore Episodes"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (_configProvider.AutoIgnorePreviouslyDownloadedEpisodes)
            {
                Logger.Info("Ignoring Previously Downloaded Episodes");
                _episodeProvider.SetPreviouslyDownloadedToIgnored();
            }
        }
    }
}
