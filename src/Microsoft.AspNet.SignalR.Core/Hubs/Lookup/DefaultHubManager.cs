// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public class DefaultHubManager : IHubManager
    {
        private readonly IEnumerable<IMethodDescriptorProvider> _methodProviders;
        private readonly IHubActivator _activator;
        private readonly IEnumerable<IHubDescriptorProvider> _hubProviders;

        public DefaultHubManager(IDependencyResolver resolver)
        {
            _hubProviders = resolver.ResolveAll<IHubDescriptorProvider>();
            _methodProviders = resolver.ResolveAll<IMethodDescriptorProvider>();
            _activator = resolver.Resolve<IHubActivator>();
        }

        public HubDescriptor GetHub(string hubName)
        {
            HubDescriptor descriptor = null;
            if (_hubProviders.FirstOrDefault(p => p.TryGetHub(hubName, out descriptor)) != null)
            {
                return descriptor;
            }

            return null;
        }

        public IEnumerable<HubDescriptor> GetHubs(Func<HubDescriptor, bool> predicate)
        {
            var hubs = _hubProviders.SelectMany(p => p.GetHubs());

            if (predicate != null)
            {
                return hubs.Where(predicate);
            }

            return hubs;
        }

        public MethodDescriptor GetHubMethod(string hubName, string method, IList<IJsonValue> parameters)
        {
            HubDescriptor hub = GetHub(hubName);

            if (hub == null)
            {
                return null;
            }

            MethodDescriptor descriptor = null;
            if (_methodProviders.FirstOrDefault(p => p.TryGetMethod(hub, method, out descriptor, parameters)) != null)
            {
                return descriptor;
            }

            return null;
        }

        public IEnumerable<MethodDescriptor> GetHubMethods(string hubName, Func<MethodDescriptor, bool> predicate)
        {
            HubDescriptor hub = GetHub(hubName);

            if (hub == null)
            {
                return null;
            }

            var methods = _methodProviders.SelectMany(p => p.GetMethods(hub));

            if (predicate != null)
            {
                return methods.Where(predicate);
            }

            return methods;

        }

        public IHub ResolveHub(string hubName)
        {
            HubDescriptor hub = GetHub(hubName);
            return hub == null ? null : _activator.Create(hub);
        }

        public IEnumerable<IHub> ResolveHubs()
        {
            return GetHubs(predicate: null).Select(hub => _activator.Create(hub));
        }
    }
}
