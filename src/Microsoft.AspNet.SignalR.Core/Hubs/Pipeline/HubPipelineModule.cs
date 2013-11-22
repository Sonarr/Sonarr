// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Common base class to simplify the implementation of IHubPipelineModules.
    /// A module can intercept and customize various stages of hub processing such as connecting, reconnecting, disconnecting,
    /// invoking server-side hub methods, invoking client-side hub methods, authorizing hub clients and rejoining hub groups.
    /// A module can be activated by calling <see cref="IHubPipeline.AddModule"/>.
    /// The combined modules added to the <see cref="IHubPipeline" /> are invoked via the <see cref="IHubPipelineInvoker"/>
    /// interface.
    /// </summary>
    public abstract class HubPipelineModule : IHubPipelineModule
    {
        /// <summary>
        /// Wraps a function that invokes a server-side hub method. Even if a client has not been authorized to connect
        /// to a hub, it will still be authorized to invoke server-side methods on that hub unless it is prevented in
        /// <see cref="IHubPipelineModule.BuildIncoming"/> by not executing the invoke parameter.
        /// </summary>
        /// <param name="invoke">A function that invokes a server-side hub method.</param>
        /// <returns>A wrapped function that invokes a server-side hub method.</returns>
        public virtual Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke)
        {
            return context =>
            {
                if (OnBeforeIncoming(context))
                {
                    return invoke(context).OrEmpty()
                                          .Then(result => OnAfterIncoming(result, context))
                                          .Catch(ex => OnIncomingError(ex, context));
                }

                return TaskAsyncHelper.FromResult<object>(null);
            };
        }

        /// <summary>
        /// Wraps a function that is called when a client connects to the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client connects to. By default, this results in the <see cref="IHub"/>'s
        /// OnConnected method being invoked.
        /// </summary>
        /// <param name="connect">A function to be called when a client connects to a hub.</param>
        /// <returns>A wrapped function to be called when a client connects to a hub.</returns>
        public virtual Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
        {
            return hub =>
            {
                if (OnBeforeConnect(hub))
                {
                    return connect(hub).OrEmpty().Then(h => OnAfterConnect(h), hub);
                }

                return TaskAsyncHelper.Empty;
            };
        }

        /// <summary>
        /// Wraps a function that is called when a client reconnects to the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client connects to. By default, this results in the <see cref="IHub"/>'s
        /// OnReconnected method being invoked.
        /// </summary>
        /// <param name="reconnect">A function to be called when a client reconnects to a hub.</param>
        /// <returns>A wrapped function to be called when a client reconnects to a hub.</returns>
        public virtual Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect)
        {
            return (hub) =>
            {
                if (OnBeforeReconnect(hub))
                {
                    return reconnect(hub).OrEmpty().Then(h => OnAfterReconnect(h), hub);
                }
                return TaskAsyncHelper.Empty;
            };
        }

        /// <summary>
        /// Wraps a function that is called  when a client disconnects from the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client was connected to. By default, this results in the <see cref="IHub"/>'s
        /// OnDisconnected method being invoked.
        /// </summary>
        /// <param name="disconnect">A function to be called when a client disconnects from a hub.</param>
        /// <returns>A wrapped function to be called when a client disconnects from a hub.</returns>
        public virtual Func<IHub, Task> BuildDisconnect(Func<IHub, Task> disconnect)
        {
            return hub =>
            {
                if (OnBeforeDisconnect(hub))
                {
                    return disconnect(hub).OrEmpty().Then(h => OnAfterDisconnect(h), hub);
                }

                return TaskAsyncHelper.Empty;
            };
        }

        /// <summary>
        /// Wraps a function to be called before a client subscribes to signals belonging to the hub described by the
        /// <see cref="HubDescriptor"/>. By default, the <see cref="AuthorizeModule"/> will look for attributes on the
        /// <see cref="IHub"/> to help determine if the client is authorized to subscribe to method invocations for the
        /// described hub.
        /// The function returns true if the client is authorized to subscribe to client-side hub method
        /// invocations; false, otherwise.
        /// </summary>
        /// <param name="authorizeConnect">
        /// A function that dictates whether or not the client is authorized to connect to the described Hub.
        /// </param>
        /// <returns>
        /// A wrapped function that dictates whether or not the client is authorized to connect to the described Hub.
        /// </returns>
        public virtual Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
        {
            return (hubDescriptor, request) =>
            {
                if (OnBeforeAuthorizeConnect(hubDescriptor, request))
                {
                    return authorizeConnect(hubDescriptor, request);
                }
                return false;
            };
        }

        /// <summary>
        /// Wraps a function that determines which of the groups belonging to the hub described by the <see cref="HubDescriptor"/>
        /// the client should be allowed to rejoin.
        /// By default, clients will rejoin all the groups they were in prior to reconnecting.
        /// </summary>
        /// <param name="rejoiningGroups">A function that determines which groups the client should be allowed to rejoin.</param>
        /// <returns>A wrapped function that determines which groups the client should be allowed to rejoin.</returns>
        public virtual Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups)
        {
            return rejoiningGroups;
        }

        /// <summary>
        /// Wraps a function that invokes a client-side hub method.
        /// </summary>
        /// <param name="send">A function that invokes a client-side hub method.</param>
        /// <returns>A wrapped function that invokes a client-side hub method.</returns>
        public virtual Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send)
        {
            return context =>
            {
                if (OnBeforeOutgoing(context))
                {
                    return send(context).OrEmpty().Then(ctx => OnAfterOutgoing(ctx), context);
                }

                return TaskAsyncHelper.Empty;
            };
        }

        /// <summary>
        /// This method is called before the AuthorizeConnect components of any modules added later to the <see cref="IHubPipeline"/>
        /// are executed. If this returns false, then those later-added modules will not run and the client will not be allowed
        /// to subscribe to client-side invocations of methods belonging to the hub defined by the <see cref="HubDescriptor"/>.
        /// </summary>
        /// <param name="hubDescriptor">A description of the hub the client is trying to subscribe to.</param>
        /// <param name="request">The connect request of the client trying to subscribe to the hub.</param>
        /// <returns>true, if the client is authorized to connect to the hub, false otherwise.</returns>
        protected virtual bool OnBeforeAuthorizeConnect(HubDescriptor hubDescriptor, IRequest request)
        {
            return true;
        }
        
        /// <summary>
        /// This method is called before the connect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. If this returns false, then those later-added modules and the <see cref="IHub.OnConnected"/> method will
        /// not be run.
        /// </summary>
        /// <param name="hub">The hub the client has connected to.</param>
        /// <returns>
        /// true, if the connect components of later added modules and the <see cref="IHub.OnConnected"/> method should be executed;
        /// false, otherwise.
        /// </returns>
        protected virtual bool OnBeforeConnect(IHub hub)
        {
            return true;
        }

        /// <summary>
        /// This method is called after the connect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed and after <see cref="IHub.OnConnected"/> is executed, if at all.
        /// </summary>
        /// <param name="hub">The hub the client has connected to.</param>
        protected virtual void OnAfterConnect(IHub hub)
        {

        }

        /// <summary>
        /// This method is called before the reconnect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. If this returns false, then those later-added modules and the <see cref="IHub.OnReconnected"/> method will
        /// not be run.
        /// </summary>
        /// <param name="hub">The hub the client has reconnected to.</param>
        /// <returns>
        /// true, if the reconnect components of later added modules and the <see cref="IHub.OnReconnected"/> method should be executed;
        /// false, otherwise.
        /// </returns>
        protected virtual bool OnBeforeReconnect(IHub hub)
        {
            return true;
        }

        /// <summary>
        /// This method is called after the reconnect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed and after <see cref="IHub.OnReconnected"/> is executed, if at all.
        /// </summary>
        /// <param name="hub">The hub the client has reconnected to.</param>
        protected virtual void OnAfterReconnect(IHub hub)
        {

        }

        /// <summary>
        /// This method is called before the outgoing components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. If this returns false, then those later-added modules and the client-side hub method invocation(s) will not
        /// be executed.
        /// </summary>
        /// <param name="context">A description of the client-side hub method invocation.</param>
        /// <returns>
        /// true, if the outgoing components of later added modules and the client-side hub method invocation(s) should be executed;
        /// false, otherwise.
        /// </returns>
        protected virtual bool OnBeforeOutgoing(IHubOutgoingInvokerContext context)
        {
            return true;
        }

        /// <summary>
        /// This method is called after the outgoing components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. This does not mean that all the clients have received the hub method invocation, but it does indicate indicate
        /// a hub invocation message has successfully been published to a message bus.
        /// </summary>
        /// <param name="context">A description of the client-side hub method invocation.</param>
        protected virtual void OnAfterOutgoing(IHubOutgoingInvokerContext context)
        {

        }

        /// <summary>
        /// This method is called before the disconnect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. If this returns false, then those later-added modules and the <see cref="IHub.OnDisconnected"/> method will
        /// not be run.
        /// </summary>
        /// <param name="hub">The hub the client has disconnected from.</param>
        /// <returns>
        /// true, if the disconnect components of later added modules and the <see cref="IHub.OnDisconnected"/> method should be executed;
        /// false, otherwise.
        /// </returns>
        protected virtual bool OnBeforeDisconnect(IHub hub)
        {
            return true;
        }

        /// <summary>
        /// This method is called after the disconnect components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed and after <see cref="IHub.OnDisconnected"/> is executed, if at all.
        /// </summary>
        /// <param name="hub">The hub the client has disconnected from.</param>
        protected virtual void OnAfterDisconnect(IHub hub)
        {

        }

        /// <summary>
        /// This method is called before the incoming components of any modules added later to the <see cref="IHubPipeline"/> are
        /// executed. If this returns false, then those later-added modules and the server-side hub method invocation will not
        /// be executed. Even if a client has not been authorized to connect to a hub, it will still be authorized to invoke
        /// server-side methods on that hub unless it is prevented in <see cref="IHubPipelineModule.BuildIncoming"/> by not
        /// executing the invoke parameter or prevented in <see cref="HubPipelineModule.OnBeforeIncoming"/> by returning false.
        /// </summary>
        /// <param name="context">A description of the server-side hub method invocation.</param>
        /// <returns>
        /// true, if the incoming components of later added modules and the server-side hub method invocation should be executed;
        /// false, otherwise.
        /// </returns>
        protected virtual bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            return true;
        }

        /// <summary>
        /// This method is called after the incoming components of any modules added later to the <see cref="IHubPipeline"/>
        /// and the server-side hub method have completed execution.
        /// </summary>
        /// <param name="result">The return value of the server-side hub method</param>
        /// <param name="context">A description of the server-side hub method invocation.</param>
        /// <returns>The possibly new or updated return value of the server-side hub method</returns>
        protected virtual object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            return result;
        }

        /// <summary>
        /// This is called when an uncaught exception is thrown by a server-side hub method or the incoming component of a
        /// module added later to the <see cref="IHubPipeline"/>. Observing the exception using this method will not prevent
        /// it from bubbling up to other modules.
        /// </summary>
        /// <param name="ex">The exception that was thrown during the server-side invocation.</param>
        /// <param name="context">A description of the server-side hub method invocation.</param>
        protected virtual void OnIncomingError(Exception ex, IHubIncomingInvokerContext context)
        {

        }
    }
}
