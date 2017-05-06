using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.SkyhookNotifications
{
    public interface ISubstituteIndexerUrl
    {
        string SubstituteUrl(string baseUrl);
    }

    public class SubstituteUrlService : ISubstituteIndexerUrl
    {
        private readonly ISkyhookNotificationService _notificationService;

        public SubstituteUrlService(ISkyhookNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public string SubstituteUrl(string baseUrl)
        {
            foreach (var action in _notificationService.GetUrlNotifications())
            {
                if (action.Type == SkyhookNotificationType.UrlBlacklist)
                {
                    if (action.RegexMatch == null) continue;

                    if (Regex.IsMatch(baseUrl, action.RegexMatch))
                    {
                        return null;
                    }
                }
                else if (action.Type == SkyhookNotificationType.UrlReplace)
                {
                    if (action.RegexMatch == null) continue;
                    if (action.RegexReplace == null) continue;

                    if (Regex.IsMatch(baseUrl, action.RegexMatch))
                    {
                        return Regex.Replace(baseUrl, action.RegexMatch, action.RegexReplace);
                    }
                }
            }

            return baseUrl;
        }
    }
}
