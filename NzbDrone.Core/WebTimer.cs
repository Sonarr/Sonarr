using System;
using System.Web;
using System.Web.Caching;
using NLog;
using NzbDrone.Core.Jobs;

namespace NzbDrone.Core
{
    public class WebTimer
    {
        private readonly JobController _jobProvider;

        private static CacheItemRemovedCallback _onCacheRemove;
        private static bool _stop;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        public WebTimer(JobController jobProvider)
        {
            _jobProvider = jobProvider;
        }

        //TODO: Fix this so the timer doesn't keep running during unit tests.
        public void StartTimer(int secondInterval)
        {
            _onCacheRemove = DoWork;

            HttpRuntime.Cache.Insert(GetType().ToString(), secondInterval, null,
                DateTime.Now.AddSeconds(secondInterval), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, _onCacheRemove);
        }


        public void DoWork(string k, object v, CacheItemRemovedReason r)
        {
            if (!_stop)
            {
                _jobProvider.QueueScheduled();
                StartTimer(Convert.ToInt32(v));
            }
        }

        public static void Stop()
        {
            Logger.Info("Stopping Web Timer");
            _stop = true;
        }
    }
}
