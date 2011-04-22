using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using NLog;

namespace NzbDrone.Core
{
    class WebTimer
    {

        private static CacheItemRemovedCallback _onCacheRemove;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void StartTimer(int secondInterval)
        {
            _onCacheRemove = new CacheItemRemovedCallback(DoWork);

            HttpRuntime.Cache.Insert(GetType().ToString(), secondInterval, null,
                DateTime.Now.AddSeconds(secondInterval), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, _onCacheRemove);
        }


        public void DoWork(string k, object v, CacheItemRemovedReason r)
        {
            Logger.Info("Tick!");
            StartTimer(Convert.ToInt32(v));
        }
    }
}
