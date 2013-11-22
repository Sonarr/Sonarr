// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// This module is added the the HubPipeline by default.
    /// 
    /// Hub level attributes that implement <see cref="IAuthorizeHubConnection"/> such as <see cref="AuthorizeAttribute"/> are applied to determine
    /// whether to allow potential clients to receive messages sent from that hub using a <see cref="HubContext"/> or a <see cref="HubConnectionContext"/>
    /// All applicable hub attributes must allow hub connection for the connection to be authorized.
    /// 
    /// Hub and method level attributes that implement <see cref="IAuthorizeHubMethodInvocation"/> such as <see cref="AuthorizeAttribute"/> are applied
    /// to determine whether to allow callers to invoke hub methods.
    /// All applicable hub level AND method level attributes must allow hub method invocation for the invocation to be authorized.
    ///
    /// Optionally, this module may be instantiated with <see cref="IAuthorizeHubConnection"/> and <see cref="IAuthorizeHubMethodInvocation"/>
    /// authorizers that will be applied globally to all hubs and hub methods.
    /// </summary>
    public class AuthorizeModule : HubPipelineModule
    {
        // Global authorizers
        private readonly IAuthorizeHubConnection _globalConnectionAuthorizer;
        private readonly IAuthorizeHubMethodInvocation _globalInvocationAuthorizer;

        // Attribute authorizer caches
        private readonly ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubConnection>> _connectionAuthorizersCache;
        private readonly ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubMethodInvocation>> _classInvocationAuthorizersCache;
        private readonly ConcurrentDictionary<MethodDescriptor, IEnumerable<IAuthorizeHubMethodInvocation>> _methodInvocationAuthorizersCache;

        // By default, this module does not include any authorizers that are applied globally.
        // This module will always apply authorizers attached to hubs or hub methods
        public AuthorizeModule()
            : this(globalConnectionAuthorizer: null, globalInvocationAuthorizer: null)
        {
        }

        public AuthorizeModule(IAuthorizeHubConnection globalConnectionAuthorizer, IAuthorizeHubMethodInvocation globalInvocationAuthorizer)
        {
            // Set global authorizers
            _globalConnectionAuthorizer = globalConnectionAuthorizer;
            _globalInvocationAuthorizer = globalInvocationAuthorizer;

            // Initialize attribute authorizer caches
            _connectionAuthorizersCache = new ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubConnection>>();
            _classInvocationAuthorizersCache = new ConcurrentDictionary<Type, IEnumerable<IAuthorizeHubMethodInvocation>>();
            _methodInvocationAuthorizersCache = new ConcurrentDictionary<MethodDescriptor, IEnumerable<IAuthorizeHubMethodInvocation>>();
        }

        public override Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
        {
            return base.BuildAuthorizeConnect((hubDescriptor, request) =>
            {
                // Execute custom modules first and short circuit if any deny authorization.
                if (!authorizeConnect(hubDescriptor, request))
                {
                    return false;
                }

                // Execute the global hub connection authorizer if there is one next and short circuit if it denies authorization.
                if (_globalConnectionAuthorizer != null && !_globalConnectionAuthorizer.AuthorizeHubConnection(hubDescriptor, request))
                {
                    return false;
                }

                // Get hub attributes implementing IAuthorizeHubConnection from the cache
                // If the attributes do not exist in the cache, retrieve them using reflection and add them to the cache
                var attributeAuthorizers = _connectionAuthorizersCache.GetOrAdd(hubDescriptor.HubType,
                    hubType => hubType.GetCustomAttributes(typeof(IAuthorizeHubConnection), inherit: true).Cast<IAuthorizeHubConnection>());

                // Every attribute (if any) implementing IAuthorizeHubConnection attached to the relevant hub MUST allow the connection
                return attributeAuthorizers.All(a => a.AuthorizeHubConnection(hubDescriptor, request));
            });
        }

        public override Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
        {
            return base.BuildIncoming(context =>
            {
                // Execute the global method invocation authorizer if there is one and short circuit if it denies authorization.
                if (_globalInvocationAuthorizer == null || _globalInvocationAuthorizer.AuthorizeHubMethodInvocation(context, appliesToMethod: false))
                {
                    // Get hub attributes implementing IAuthorizeHubMethodInvocation from the cache
                    // If the attributes do not exist in the cache, retrieve them using reflection and add them to the cache
                    var classLevelAuthorizers = _classInvocationAuthorizersCache.GetOrAdd(context.Hub.GetType(),
                        hubType => hubType.GetCustomAttributes(typeof(IAuthorizeHubMethodInvocation), inherit: true).Cast<IAuthorizeHubMethodInvocation>());

                    // Execute all hub level authorizers and short circuit if ANY deny authorization.
                    if (classLevelAuthorizers.All(a => a.AuthorizeHubMethodInvocation(context, appliesToMethod: false)))
                    {
                        // If the MethodDescriptor is a NullMethodDescriptor, we don't want to cache it since a new one is created
                        // for each invocation with an invalid method name. #1801
                        if (context.MethodDescriptor is NullMethodDescriptor)
                        {
                            return invoke(context);
                        }

                        // Get method attributes implementing IAuthorizeHubMethodInvocation from the cache
                        // If the attributes do not exist in the cache, retrieve them from the MethodDescriptor and add them to the cache
                        var methodLevelAuthorizers = _methodInvocationAuthorizersCache.GetOrAdd(context.MethodDescriptor,
                            methodDescriptor => methodDescriptor.Attributes.OfType<IAuthorizeHubMethodInvocation>());
                        
                        // Execute all method level authorizers. If ALL provide authorization, continue executing the invocation pipeline.
                        if (methodLevelAuthorizers.All(a => a.AuthorizeHubMethodInvocation(context, appliesToMethod: true)))
                        {
                            return invoke(context);
                        }
                    }
                }
                
                // Send error back to the client
                return TaskAsyncHelper.FromError<object>(
                    new NotAuthorizedException(String.Format(CultureInfo.CurrentCulture, Resources.Error_CallerNotAuthorizedToInvokeMethodOn,
                                                             context.MethodDescriptor.Name,
                                                             context.MethodDescriptor.Hub.Name)));
            });
        }
    }
}
