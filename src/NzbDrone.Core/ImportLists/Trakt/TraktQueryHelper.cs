using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public static class TraktQueryHelper
    {
        private static readonly HashSet<string> CommaSeparatedParams = new(StringComparer.OrdinalIgnoreCase)
        {
            "genres",
            "certifications",
            "networks",
            "languages",
            "countries",
            "status"
        };

        public static Dictionary<string, string> BuildFilterParameters(string rating, string genres, string years, int limit, string additionalParameters)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Parse additional parameters first (lower priority)
            if (additionalParameters.IsNotNullOrWhiteSpace())
            {
                var trimmed = additionalParameters.TrimStart('?').TrimStart('&');

                foreach (var param in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = param.Split('=', 2);

                    if (parts.Length == 2 && parts[0].IsNotNullOrWhiteSpace())
                    {
                        parameters[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            // Apply explicit settings (higher priority)
            // For comma-separated params like genres, combine values from both sources
            if (genres.IsNotNullOrWhiteSpace())
            {
                if (parameters.TryGetValue("genres", out var existingGenres) && existingGenres.IsNotNullOrWhiteSpace())
                {
                    var allGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var g in genres.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        allGenres.Add(g.Trim());
                    }

                    foreach (var g in existingGenres.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        allGenres.Add(g.Trim());
                    }

                    parameters["genres"] = string.Join(",", allGenres);
                }
                else
                {
                    parameters["genres"] = genres.ToLower();
                }
            }

            // For ratings and years, explicit settings override additional parameters
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

        public static string ToQueryString(this Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Where(p => p.Value.IsNotNullOrWhiteSpace()).Select(p => $"{p.Key}={p.Value}"));
        }
    }
}
