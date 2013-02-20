using System.Linq;
using System;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Converting;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class ConvertEpisodeJob : IJob
    {
        private readonly HandbrakeProvider _handbrakeProvider;
        private readonly AtomicParsleyProvider _atomicParsleyProvider;
        private readonly EpisodeService _episodeService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ConvertEpisodeJob(HandbrakeProvider handbrakeProvider, AtomicParsleyProvider atomicParsleyProvider,
                                    EpisodeService episodeService)
        {
            _handbrakeProvider = handbrakeProvider;
            _atomicParsleyProvider = atomicParsleyProvider;
            _episodeService = episodeService;
        }

        public string Name
        {
            get { return "Convert Episode"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {

            if (options == null || options.EpisodeId <= 0)
                throw new ArgumentNullException(options);

            Episode episode = _episodeService.GetEpisode(options.EpisodeId);
            notification.CurrentMessage = String.Format("Starting Conversion for {0}", episode);
            var outputFile = _handbrakeProvider.ConvertFile(episode, notification);

            if (String.IsNullOrEmpty(outputFile))
                notification.CurrentMessage = String.Format("Conversion failed for {0}", episode);

            _atomicParsleyProvider.RunAtomicParsley(episode, outputFile);

            notification.CurrentMessage = String.Format("Conversion completed for {0}", episode);
        }
    }
}