using System;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public static class UriExtensions
    {
        public static void SetQueryParam(this UriBuilder uriBuilder, string key, object value)
        {
            var query = uriBuilder.Query;

            if (query.IsNotNullOrWhiteSpace())
            {
                query += "&";
            }

            uriBuilder.Query = query.Trim('?') + key + "=" + Uri.EscapeDataString(value.ToString());
        }
    }
}