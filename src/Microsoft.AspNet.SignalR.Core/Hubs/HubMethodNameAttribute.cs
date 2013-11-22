// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HubMethodNameAttribute : Attribute
    {
        public HubMethodNameAttribute(string methodName)
        {
            if (String.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }
            MethodName = methodName;
        }

        public string MethodName
        {
            get;
            private set;
        }
    }
}
