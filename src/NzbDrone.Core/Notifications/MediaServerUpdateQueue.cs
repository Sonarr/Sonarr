using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class MediaServerUpdateQueue<TQueueHost, TItemInfo>
        where TQueueHost : class
    {
        private class UpdateQueue
        {
            public Dictionary<int, UpdateQueueItem<TItemInfo>> Pending { get; } = new Dictionary<int, UpdateQueueItem<TItemInfo>>();
            public bool Refreshing { get; set; }
        }

        private readonly ICached<UpdateQueue> _pendingSeriesCache;

        public MediaServerUpdateQueue(ICacheManager cacheManager)
        {
            _pendingSeriesCache = cacheManager.GetRollingCache<UpdateQueue>(typeof(TQueueHost), "pendingSeries", TimeSpan.FromDays(1));
        }

        public void Add(string identifier, Series series, TItemInfo info)
        {
            var queue = _pendingSeriesCache.Get(identifier, () => new UpdateQueue());

            lock (queue)
            {
                var item = queue.Pending.TryGetValue(series.Id, out var value)
                    ? value
                    : new UpdateQueueItem<TItemInfo>(series);

                item.Info.Add(info);

                queue.Pending[series.Id] = item;
            }
        }

        public void ProcessQueue(string identifier, Action<List<UpdateQueueItem<TItemInfo>>> update)
        {
            var queue = _pendingSeriesCache.Find(identifier);

            if (queue == null)
            {
                return;
            }

            lock (queue)
            {
                if (queue.Refreshing)
                {
                    return;
                }

                queue.Refreshing = true;
            }

            try
            {
                while (true)
                {
                    List<UpdateQueueItem<TItemInfo>> items;

                    lock (queue)
                    {
                        if (queue.Pending.Empty())
                        {
                            queue.Refreshing = false;
                            return;
                        }

                        items = queue.Pending.Values.ToList();
                        queue.Pending.Clear();
                    }

                    update(items);
                }
            }
            catch
            {
                lock (queue)
                {
                    queue.Refreshing = false;
                }

                throw;
            }
        }
    }

    public class UpdateQueueItem<TItemInfo>
    {
        public Series Series { get; set; }
        public HashSet<TItemInfo> Info { get; set; }

        public UpdateQueueItem(Series series)
        {
            Series = series;
            Info = new HashSet<TItemInfo>();
        }
    }
}
