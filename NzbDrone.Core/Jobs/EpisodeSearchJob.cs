using System.Linq;
using System;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class EpisodeSearchJob : IJob
    {
        private readonly SearchProvider _searchProvider;

        [Inject]
        public EpisodeSearchJob(SearchProvider searchProvider)
        {
            _searchProvider = searchProvider;
        }

        public EpisodeSearchJob()
        {
            
        }

        public string Name
        {
            get { return "Episode Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null || options.EpisodeId <= 0)
                throw new ArgumentException("options");

            _searchProvider.EpisodeSearch(notification, options.EpisodeId);
        }
    }
}