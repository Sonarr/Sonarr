// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal class ListHelper<T>
    {
        public static readonly IList<T> Empty = new ReadOnlyCollection<T>(new List<T>());
    }
}
