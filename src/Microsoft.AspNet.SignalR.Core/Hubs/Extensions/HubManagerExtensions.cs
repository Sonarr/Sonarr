// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public static class HubManagerExtensions
    {
        public static HubDescriptor EnsureHub(this IHubManager hubManager, string hubName, params IPerformanceCounter[] counters)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            if (String.IsNullOrEmpty(hubName))
            {
                throw new ArgumentNullException("hubName");
            }

            if (counters == null)
            {
                throw new ArgumentNullException("counters");
            }

            var descriptor = hubManager.GetHub(hubName);

            if (descriptor == null)
            {
                for (var i = 0; i < counters.Length; i++)
                {
                    counters[i].Increment();
                }
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Error_HubCouldNotBeResolved, hubName));
            }

            return descriptor;
        }

        public static IEnumerable<HubDescriptor> GetHubs(this IHubManager hubManager)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            return hubManager.GetHubs(d => true);
        }

        public static IEnumerable<MethodDescriptor> GetHubMethods(this IHubManager hubManager, string hubName)
        {
            if (hubManager == null)
            {
                throw new ArgumentNullException("hubManager");
            }

            return hubManager.GetHubMethods(hubName, m => true);
        }
    }
}
