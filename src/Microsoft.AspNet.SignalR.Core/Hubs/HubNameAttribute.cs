// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HubNameAttribute : Attribute
    {
        public HubNameAttribute(string hubName)
        {
            if (String.IsNullOrEmpty(hubName))
            {
                throw new ArgumentNullException("hubName");
            }
            HubName = hubName;
        }

        public string HubName
        {
            get;
            private set;
        }
    }
}
