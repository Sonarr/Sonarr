// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Owin
{
    internal static class OwinConstants
    {
        public const string Version = "owin.Version";

        public const string RequestBody = "owin.RequestBody";
        public const string RequestHeaders = "owin.RequestHeaders";
        public const string RequestScheme = "owin.RequestScheme";
        public const string RequestMethod = "owin.RequestMethod";
        public const string RequestPathBase = "owin.RequestPathBase";
        public const string RequestPath = "owin.RequestPath";
        public const string RequestQueryString = "owin.RequestQueryString";
        public const string RequestProtocol = "owin.RequestProtocol";

        public const string CallCancelled = "owin.CallCancelled";

        public const string ResponseStatusCode = "owin.ResponseStatusCode";
        public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";
        public const string ResponseHeaders = "owin.ResponseHeaders";
        public const string ResponseBody = "owin.ResponseBody";

        public const string TraceOutput = "host.TraceOutput";

        public const string User = "server.User";
        public const string RemoteIpAddress = "server.RemoteIpAddress";
        public const string RemotePort = "server.RemotePort";
        public const string LocalIpAddress = "server.LocalIpAddress";
        public const string LocalPort = "server.LocalPort";

        public const string DisableRequestCompression = "systemweb.DisableResponseCompression";
        public const string DisableRequestBuffering = "server.DisableRequestBuffering";
        public const string DisableResponseBuffering = "server.DisableResponseBuffering";

        public const string ServerCapabilities = "server.Capabilities";
        public const string WebSocketVersion = "websocket.Version";
        public const string WebSocketAccept = "websocket.Accept";

        public const string HostOnAppDisposing = "host.OnAppDisposing";
        public const string HostAppNameKey = "host.AppName";
        public const string HostAppModeKey = "host.AppMode";
        public const string AppModeDevelopment = "development";
    }
}
