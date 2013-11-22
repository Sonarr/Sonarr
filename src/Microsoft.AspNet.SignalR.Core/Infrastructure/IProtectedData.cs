// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public interface IProtectedData
    {
        string Protect(string data, string purpose);
        string Unprotect(string protectedValue, string purpose);
    }
}
