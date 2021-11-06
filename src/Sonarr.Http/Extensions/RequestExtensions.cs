using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Exceptions;

namespace Sonarr.Http.Extensions
{
    public static class RequestExtensions
    {
        // See src/Readarr.Api.V1/Queue/QueueModule.cs
        private static readonly HashSet<string> VALID_SORT_KEYS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "series.sortname", //Workaround authors table properties not being added on isValidSortKey call
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

        public static PagingResource<TResource> ReadPagingResourceFromRequest<TResource>(this HttpRequest request)
        {
            if (!int.TryParse(request.Query["PageSize"].ToString(), out var pageSize))
            {
                pageSize = 10;
            }

            if (!int.TryParse(request.Query["Page"].ToString(), out var page))
            {
                page = 1;
            }

            var pagingResource = new PagingResource<TResource>
            {
                PageSize = pageSize,
                Page = page,
                Filters = new List<PagingResourceFilter>()
            };

            if (request.Query["SortKey"].Any())
            {
                var sortKey = request.Query["SortKey"].ToString();

                if (!VALID_SORT_KEYS.Contains(sortKey) &&
                    !TableMapping.Mapper.IsValidSortKey(sortKey))
                {
                    throw new BadRequestException($"Invalid sort key {sortKey}");
                }

                pagingResource.SortKey = sortKey;

                if (request.Query["SortDirection"].Any())
                {
                    pagingResource.SortDirection = request.Query["SortDirection"].ToString()
                                                          .Equals("ascending", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }
            }

            // For backwards compatibility with v2
            if (request.Query["FilterKey"].Any())
            {
                var filter = new PagingResourceFilter
                {
                    Key = request.Query["FilterKey"].ToString()
                };

                if (request.Query["FilterValue"].Any())
                {
                    filter.Value = request.Query["FilterValue"].ToString();
                }

                pagingResource.Filters.Add(filter);
            }

            // v3 uses filters in key=value format
            foreach (var pair in request.Query)
            {
                if (EXCLUDED_KEYS.Contains(pair.Key))
                {
                    continue;
                }

                pagingResource.Filters.Add(new PagingResourceFilter
                {
                    Key = pair.Key,
                    Value = pair.Value.ToString()
                });
            }

            return pagingResource;
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

            var remoteAddress = remoteIP.ToString();

            // Only check if forwarded by a local network reverse proxy
            if (remoteIP.IsLocalAddress())
            {
                var realIPHeader = request.Headers["X-Real-IP"];
                if (realIPHeader.Any())
                {
                    return realIPHeader.First().ToString();
                }

                var forwardedForHeader = request.Headers["X-Forwarded-For"];
                if (forwardedForHeader.Any())
                {
                    // Get the first address that was forwarded by a local IP to prevent remote clients faking another proxy
                    foreach (var forwardedForAddress in forwardedForHeader.SelectMany(v => v.Split(',')).Select(v => v.Trim()).Reverse())
                    {
                        if (!IPAddress.TryParse(forwardedForAddress, out remoteIP))
                        {
                            return remoteAddress;
                        }

                        if (!remoteIP.IsLocalAddress())
                        {
                            return forwardedForAddress;
                        }

                        remoteAddress = forwardedForAddress;
                    }
                }
            }

            return remoteAddress;
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
