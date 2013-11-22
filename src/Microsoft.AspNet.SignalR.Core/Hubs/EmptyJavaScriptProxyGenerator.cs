// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class EmptyJavaScriptProxyGenerator : IJavaScriptProxyGenerator
    {
        public string GenerateProxy(string serviceUrl)
        {
            return String.Format(CultureInfo.InvariantCulture, "throw new Error('{0}');", Resources.Error_JavaScriptProxyDisabled);
        }
    }
}
