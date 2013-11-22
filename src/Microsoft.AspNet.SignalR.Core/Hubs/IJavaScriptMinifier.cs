// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Hubs
{
    public interface IJavaScriptMinifier
    {
        string Minify(string source);
    }
}
