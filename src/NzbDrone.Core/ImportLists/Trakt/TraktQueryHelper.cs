using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public static class TraktQueryHelper
    {
        private static readonly HashSet<string> ReservedFilterParameters = new(StringComparer.OrdinalIgnoreCase)
        {
            "genres",
            "ratings",
            "years",
            "limit"
        };

        public static Dictionary<string, string> BuildFilterParameters(string rating, string genres, string years, int limit, string additionalParameters)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (additionalParameters.IsNotNullOrWhiteSpace())
            {
                var trimmed = additionalParameters.TrimStart('?').TrimStart('&');

                foreach (var param in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = param.Split('=', 2);

                    if (parts.Length == 2 && parts[0].IsNotNullOrWhiteSpace())
                    {
                        var key = parts[0].Trim();

                        if (ReservedFilterParameters.Contains(key))
                        {
                            continue;
                        }

                        parameters[key] = parts[1].Trim();
                    }
                }
            }

            if (genres.IsNotNullOrWhiteSpace())
            {
                parameters["genres"] = genres.ToLower();
            }

            if (rating.IsNotNullOrWhiteSpace())
            {
                parameters["ratings"] = rating;
            }

            if (years.IsNotNullOrWhiteSpace())
            {
                parameters["years"] = years;
            }

            parameters["limit"] = limit.ToString();

            return parameters;
        }

        public static bool ContainsReservedFilterParameters(string additionalParameters)
        {
            if (additionalParameters.IsNullOrWhiteSpace())
            {
                return false;
            }

            var trimmed = additionalParameters.TrimStart('?').TrimStart('&');

            foreach (var param in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = param.Split('=', 2);

                if (parts.Length == 2 && parts[0].IsNotNullOrWhiteSpace() && ReservedFilterParameters.Contains(parts[0].Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ToQueryString(this Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Where(p => p.Value.IsNotNullOrWhiteSpace()).Select(p => $"{p.Key}={p.Value}"));
        }
    }
}
