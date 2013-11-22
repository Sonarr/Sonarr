// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.AspNet.SignalR.Transports;

namespace Microsoft.AspNet.SignalR
{
    public class DefaultDependencyResolver : IDependencyResolver
    {
        private readonly Dictionary<Type, IList<Func<object>>> _resolvers = new Dictionary<Type, IList<Func<object>>>();
        private readonly HashSet<IDisposable> _trackedDisposables = new HashSet<IDisposable>();
        private int _disposed;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "It's easiest")]
        public DefaultDependencyResolver()
        {
            RegisterDefaultServices();

            // Hubs
            RegisterHubExtensions();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "The resolver is the class that does the most coupling by design.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The resolver disposes dependencies on Dispose.")]
        private void RegisterDefaultServices()
        {
            var traceManager = new Lazy<TraceManager>(() => new TraceManager());
            Register(typeof(ITraceManager), () => traceManager.Value);

            var serverIdManager = new ServerIdManager();
            Register(typeof(IServerIdManager), () => serverIdManager);

            var serverMessageHandler = new Lazy<IServerCommandHandler>(() => new ServerCommandHandler(this));
            Register(typeof(IServerCommandHandler), () => serverMessageHandler.Value);

            var newMessageBus = new Lazy<IMessageBus>(() => new MessageBus(this));
            Register(typeof(IMessageBus), () => newMessageBus.Value);

            var stringMinifier = new Lazy<IStringMinifier>(() => new StringMinifier());
            Register(typeof(IStringMinifier), () => stringMinifier.Value);

            var serializer = new Lazy<JsonNetSerializer>();
            Register(typeof(IJsonSerializer), () => serializer.Value);

            var transportManager = new Lazy<TransportManager>(() => new TransportManager(this));
            Register(typeof(ITransportManager), () => transportManager.Value);

            var configurationManager = new DefaultConfigurationManager();
            Register(typeof(IConfigurationManager), () => configurationManager);

            var transportHeartbeat = new Lazy<TransportHeartbeat>(() => new TransportHeartbeat(this));
            Register(typeof(ITransportHeartbeat), () => transportHeartbeat.Value);

            var connectionManager = new Lazy<ConnectionManager>(() => new ConnectionManager(this));
            Register(typeof(IConnectionManager), () => connectionManager.Value);

            var ackHandler = new Lazy<AckHandler>();
            Register(typeof(IAckHandler), () => ackHandler.Value);

            var perfCounterWriter = new Lazy<PerformanceCounterManager>(() => new PerformanceCounterManager(this));
            Register(typeof(IPerformanceCounterManager), () => perfCounterWriter.Value);

            var protectedData = new DefaultProtectedData();
            Register(typeof(IProtectedData), () => protectedData);
        }

        private void RegisterHubExtensions()
        {
            var methodDescriptorProvider = new Lazy<ReflectedMethodDescriptorProvider>();
            Register(typeof(IMethodDescriptorProvider), () => methodDescriptorProvider.Value);

            var hubDescriptorProvider = new Lazy<ReflectedHubDescriptorProvider>(() => new ReflectedHubDescriptorProvider(this));
            Register(typeof(IHubDescriptorProvider), () => hubDescriptorProvider.Value);

            var parameterBinder = new Lazy<DefaultParameterResolver>();
            Register(typeof(IParameterResolver), () => parameterBinder.Value);

            var activator = new Lazy<DefaultHubActivator>(() => new DefaultHubActivator(this));
            Register(typeof(IHubActivator), () => activator.Value);

            var hubManager = new Lazy<DefaultHubManager>(() => new DefaultHubManager(this));
            Register(typeof(IHubManager), () => hubManager.Value);

            var proxyGenerator = new Lazy<DefaultJavaScriptProxyGenerator>(() => new DefaultJavaScriptProxyGenerator(this));
            Register(typeof(IJavaScriptProxyGenerator), () => proxyGenerator.Value);

            var requestParser = new Lazy<HubRequestParser>();
            Register(typeof(IHubRequestParser), () => requestParser.Value);

            var assemblyLocator = new Lazy<DefaultAssemblyLocator>(() => new DefaultAssemblyLocator());
            Register(typeof(IAssemblyLocator), () => assemblyLocator.Value);

            // Setup the default hub pipeline
            var dispatcher = new Lazy<IHubPipeline>(() => new HubPipeline().AddModule(new AuthorizeModule()));
            Register(typeof(IHubPipeline), () => dispatcher.Value);
            Register(typeof(IHubPipelineInvoker), () => dispatcher.Value);
        }

        public virtual object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            IList<Func<object>> activators;
            if (_resolvers.TryGetValue(serviceType, out activators))
            {
                if (activators.Count == 0)
                {
                    return null;
                }
                if (activators.Count > 1)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Error_MultipleActivatorsAreaRegisteredCallGetServices, serviceType.FullName));
                }
                return Track(activators[0]);
            }
            return null;
        }

        public virtual IEnumerable<object> GetServices(Type serviceType)
        {
            IList<Func<object>> activators;
            if (_resolvers.TryGetValue(serviceType, out activators))
            {
                if (activators.Count == 0)
                {
                    return null;
                }
                return activators.Select(Track).ToList();
            }
            return null;
        }

        public virtual void Register(Type serviceType, Func<object> activator)
        {
            IList<Func<object>> activators;
            if (!_resolvers.TryGetValue(serviceType, out activators))
            {
                activators = new List<Func<object>>();
                _resolvers.Add(serviceType, activators);
            }
            else
            {
                activators.Clear();
            }
            activators.Add(activator);
        }

        public virtual void Register(Type serviceType, IEnumerable<Func<object>> activators)
        {
            if (activators == null)
            {
                throw new ArgumentNullException("activators");
            }

            IList<Func<object>> list;
            if (!_resolvers.TryGetValue(serviceType, out list))
            {
                list = new List<Func<object>>();
                _resolvers.Add(serviceType, list);
            }
            else
            {
                list.Clear();
            }
            foreach (var a in activators)
            {
                list.Add(a);
            }
        }

        private object Track(Func<object> creator)
        {
            object obj = creator();

            if (_disposed == 0)
            {
                var disposable = obj as IDisposable;
                if (disposable != null)
                {
                    lock (_trackedDisposables)
                    {
                        if (_disposed == 0)
                        {
                            _trackedDisposables.Add(disposable);
                        }
                    }
                }
            }

            return obj;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    lock (_trackedDisposables)
                    {
                        foreach (var d in _trackedDisposables)
                        {
                            d.Dispose();
                        }

                        _trackedDisposables.Clear();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
