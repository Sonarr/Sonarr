// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.SignalR.Owin.Infrastructure
{
    /// <summary>
    /// Helper methods for creating and consuming CallParameters.Headers and ResultParameters.Headers.
    /// </summary>
    internal static class Headers
    {
        public static IDictionary<string, string[]> SetHeader(this IDictionary<string, string[]> headers,
            string name, string value)
        {
            headers[name] = new[] { value };
            return headers;
        }

        public static string[] GetHeaders(this IDictionary<string, string[]> headers,
            string name)
        {
            string[] value;
            return headers != null && headers.TryGetValue(name, out value) ? value : null;
        }

        public static string GetHeader(this IDictionary<string, string[]> headers,
            string name)
        {
            var values = GetHeaders(headers, name);
            if (values == null)
            {
                return null;
            }

            switch (values.Length)
            {
                case 0:
                    return String.Empty;
                case 1:
                    return values[0];
                default:
                    return String.Join(",", values);
            }
        }
    }
}
