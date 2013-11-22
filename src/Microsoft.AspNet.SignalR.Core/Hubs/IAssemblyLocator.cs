// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public interface IAssemblyLocator
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Might be expensive.")]
        IList<Assembly> GetAssemblies();
    }
}
