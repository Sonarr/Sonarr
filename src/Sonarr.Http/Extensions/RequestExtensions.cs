using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;

namespace Sonarr.Http.Extensions
{
    public static class RequestExtensions
    {
        // See src/Readarr.Api.V1/Queue/QueueModule.cs
        private static readonly HashSet<string> VALID_SORT_KEYS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "series.sortname", // Workaround authors table properties not being added on isValidSortKey call
            "episode.title", // Deprecated
            "episode.airDateUtc", // Deprecated
            "episode.language", // Deprecated
            "timeleft",
            "estimatedCompletionTime",
            "protocol",
            "episode",
            "indexer",
            "downloadClient",
            "quality",
            "status",
            "title",
            "progress"
        };

        private static readonly HashSet<string> EXCLUDED_KEYS = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "page",
            "pageSize",
            "sortKey",
            "sortDirection",
            "filterKey",
            "filterValue",
        };

        public static bool IsApiRequest(this HttpRequest request)
        {
            return request.Path.StartsWithSegments("/api", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFavIconRequest(this HttpRequest request)
        {
            return request.Path.Equals("/favicon.ico", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool GetBooleanQueryParameter(this HttpRequest request, string parameter, bool defaultValue = false)
        {
            var parameterValue = request.Query[parameter];

            if (parameterValue.Any())
            {
                return bool.Parse(parameterValue.ToString());
            }

            return defaultValue;
        }

        public static PagingResource<TResource> ApplyToPage<TResource, TModel>(this PagingSpec<TModel> pagingSpec, Func<PagingSpec<TModel>, PagingSpec<TModel>> function, Converter<TModel, TResource> mapper)
        {
            pagingSpec = function(pagingSpec);

            return new PagingResource<TResource>
            {
                Page = pagingSpec.Page,
                PageSize = pagingSpec.PageSize,
                SortDirection = pagingSpec.SortDirection,
                SortKey = pagingSpec.SortKey,
                TotalRecords = pagingSpec.TotalRecords,
                Records = pagingSpec.Records.ConvertAll(mapper)
            };
        }

        public static string GetRemoteIP(this HttpContext context)
        {
            return context?.Request?.GetRemoteIP() ?? "Unknown";
        }

        public static string GetRemoteIP(this HttpRequest request)
        {
            if (request == null)
            {
                return "Unknown";
            }

            var remoteIP = request.HttpContext.Connection.RemoteIpAddress;

            if (remoteIP.IsIPv4MappedToIPv6)
            {
                remoteIP = remoteIP.MapToIPv4();
            }

            return remoteIP.ToString();
        }

        public static string GetSource(this HttpRequest request)
        {
            if (request.Headers.TryGetValue("X-Sonarr-Client", out var source))
            {
                return source;
            }

            return NzbDrone.Common.Http.UserAgentParser.ParseSource(request.Headers["User-Agent"]);
        }

        public static void DisableCache(this IHeaderDictionary headers)
        {
            headers.Remove("Last-Modified");
            headers["Cache-Control"] = "no-cache, no-store";
            headers["Expires"] = "-1";
            headers["Pragma"] = "no-cache";
        }

        public static void EnableCache(this IHeaderDictionary headers)
        {
            headers["Cache-Control"] = "max-age=31536000, public";
            headers["Last-Modified"] = BuildInfo.BuildDateTime.ToString("r");
        }
    }
}
