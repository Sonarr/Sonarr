using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class SearchHistoryCleanupJob : IJob
    {
        private readonly SearchHistoryProvider _searchHistoryProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SearchHistoryCleanupJob(SearchHistoryProvider searchHistoryProvider)
        {
            _searchHistoryProvider = searchHistoryProvider;
        }

        public SearchHistoryCleanupJob()
        {
        }

        public string Name
        {
            get { return "Search History Cleanup"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(24); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            Logger.Info("Running search history cleanup.");
            _searchHistoryProvider.Cleanup();
        }
    }
}
