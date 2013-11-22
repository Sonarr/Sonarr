// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Messaging
{
    internal class TopicState
    {
        public const int NoSubscriptions = 0;
        public const int HasSubscriptions = 1;
        public const int Dying = 2;
        public const int Dead = 3;
    }
}
