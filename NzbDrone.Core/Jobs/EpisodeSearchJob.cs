using System.Linq;
using System;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;

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

        public int DefaultInterval
        {
            get { return 0; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            _searchProvider.EpisodeSearch(notification, targetId);
        }
    }
}