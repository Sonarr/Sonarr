using System;
using System.Linq;

namespace NzbDrone.Core.SkyhookNotifications
{
    public enum SkyhookNotificationType
    {
        // Notification for the user alone.
        Notification = 1,

        // Indexer urls matching the RegexMatch are automatically set to temporarily disabled and never contacted.
        UrlBlacklist = 2,

        // Indexer urls matching the RegexMatch are replaced with RegexReplace.
        UrlReplace = 3
    }
}
