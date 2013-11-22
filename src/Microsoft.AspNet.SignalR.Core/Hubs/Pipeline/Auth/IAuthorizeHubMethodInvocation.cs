// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Interface to be implemented by <see cref="System.Attribute"/>s that can authorize the invocation of <see cref="IHub"/> methods.
    /// </summary>
    public interface IAuthorizeHubMethodInvocation
    {
        /// <summary>
        /// Given a <see cref="IHubIncomingInvokerContext"/>, determine whether client is authorized to invoke the <see cref="IHub"/> method.
        /// </summary>
        /// <param name="hubIncomingInvokerContext">An <see cref="IHubIncomingInvokerContext"/> providing details regarding the <see cref="IHub"/> method invocation.</param>
        /// <param name="appliesToMethod">Indicates whether the interface instance is an attribute applied directly to a method.</param>
        /// <returns>true if the caller is authorized to invoke the <see cref="IHub"/> method; otherwise, false.</returns>
        bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod);
    }
}
