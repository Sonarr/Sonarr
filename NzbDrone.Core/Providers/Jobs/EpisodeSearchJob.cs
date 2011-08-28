using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
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