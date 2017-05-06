using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.SkyhookNotifications
{
    public interface ISkyhookNotificationService
    {
        List<SkyhookNotification> GetUserNotifications();
        List<SkyhookNotification> GetUrlNotifications();
    }

    public class SkyhookNotificationService : ISkyhookNotificationService
    {
        private readonly ISkyhookNotificationProxy _proxy;
        private readonly Logger _logger;

        private readonly ICached<List<SkyhookNotification>> _cache;

        private readonly TimeSpan CacheExpiry = TimeSpan.FromHours(12);

        public SkyhookNotificationService(ISkyhookNotificationProxy proxy, ICacheManager cacheManager, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;

            _cache = cacheManager.GetCache<List<SkyhookNotification>>(GetType());
        }

        private List<SkyhookNotification> GetNotifications(string key)
        {
            var result = _cache.Find(key);

            if (result == null)
            {
                var all = _proxy.GetNotifications().Where(FilterVersion).ToList();

                var user = all.Where(v => v.Type == SkyhookNotificationType.Notification).ToList();
                var url = all.Where(v => v.Type == SkyhookNotificationType.UrlBlacklist || v.Type == SkyhookNotificationType.UrlReplace).ToList();
                _cache.Set("all", all, CacheExpiry);
                _cache.Set("user", user, CacheExpiry);
                _cache.Set("url", url, CacheExpiry);
            }

            return _cache.Find(key);
        }

        public List<SkyhookNotification> GetUserNotifications()
        {
            return GetNotifications("user");
        }

        public List<SkyhookNotification> GetUrlNotifications()
        {
            return GetNotifications("url");
        }

        private bool FilterVersion(SkyhookNotification notification)
        {
            if (notification.MinimumVersion != null && BuildInfo.Version < Version.Parse(notification.MinimumVersion))
            {
                return false;
            }

            if (notification.MaximumVersion != null && BuildInfo.Version > Version.Parse(notification.MaximumVersion))
            {
                return false;
            }

            return true;
        }
    }
}
