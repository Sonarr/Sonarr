// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class DefaultHubActivator : IHubActivator
    {
        private readonly IDependencyResolver _resolver;

        public DefaultHubActivator(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if(descriptor.HubType == null)
            {
                return null;
            }

            object hub = _resolver.Resolve(descriptor.HubType) ?? Activator.CreateInstance(descriptor.HubType);
            return hub as IHub;
        }
    }
}
