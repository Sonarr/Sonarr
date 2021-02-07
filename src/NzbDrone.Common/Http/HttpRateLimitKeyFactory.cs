using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NzbDrone.Common.Http
{
    public static class HttpRateLimitKeyFactory
    {
        // Use a different key for jackett instances to prevent hitting the ratelimit for multiple separate indexers.
        private static readonly Regex _regex = new Regex(@"^https?://(.+/jackett/api/v2.0/indexers/\w+)/", RegexOptions.Compiled);

        public static string GetRateLimitKey(HttpRequest request)
        {
            var match = _regex.Match(request.Url.ToString());

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return request.Url.Host;
        }
            
    }
}
