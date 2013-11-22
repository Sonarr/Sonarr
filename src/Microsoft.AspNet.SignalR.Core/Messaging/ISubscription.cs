// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public interface ISubscription
    {
        string Identity { get; }

        bool SetQueued();
        bool UnsetQueued();

        Task Work();
    }
}
