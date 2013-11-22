// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class NullJavaScriptMinifier : IJavaScriptMinifier
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This is a singleton")]
        public static readonly NullJavaScriptMinifier Instance = new NullJavaScriptMinifier();

        public string Minify(string source)
        {
            return source;
        }
    }
}
