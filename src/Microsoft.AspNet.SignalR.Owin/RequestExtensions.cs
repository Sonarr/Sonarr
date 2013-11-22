// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Owin;

namespace Microsoft.AspNet.SignalR
{
    public static class RequestExtensions
    {
        public static T GetOwinVariable<T>(this IRequest request, string key)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var env = request.Items.Get<IDictionary<string, object>>(ServerRequest.OwinEnvironmentKey);

            return env == null ? default(T) : env.Get<T>(key);
        }

        private static T Get<T>(this IDictionary<string, object> values, string key)
        {
            object value;
            return values.TryGetValue(key, out value) ? (T)value : default(T);
        }
    }
}
