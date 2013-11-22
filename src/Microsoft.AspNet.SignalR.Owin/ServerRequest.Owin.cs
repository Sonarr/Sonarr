// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR.Owin.Infrastructure;

namespace Microsoft.AspNet.SignalR.Owin
{
    public partial class ServerRequest
    {
        private readonly IDictionary<string, object> _environment;

        public static readonly string OwinEnvironmentKey = "owin.environment";

        public ServerRequest(IDictionary<string, object> environment)
        {
            _environment = environment;

            Items = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { OwinEnvironmentKey , _environment }
            };
        }

        private string RequestMethod
        {
            get { return _environment.Get<string>(OwinConstants.RequestMethod); }
        }

        public IDictionary<string, string[]> RequestHeaders
        {
            get { return _environment.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeaders); }
        }

        private Stream RequestBody
        {
            get { return _environment.Get<Stream>(OwinConstants.RequestBody); }
        }

        private string RequestScheme
        {
            get { return _environment.Get<string>(OwinConstants.RequestScheme); }
        }

        private string RequestPathBase
        {
            get { return _environment.Get<string>(OwinConstants.RequestPathBase); }
        }

        private string RequestPath
        {
            get { return _environment.Get<string>(OwinConstants.RequestPath); }
        }

        private string RequestQueryString
        {
            get { return _environment.Get<string>(OwinConstants.RequestQueryString); }
        }

        public Action DisableRequestCompression
        {
            get { return _environment.Get<Action>(OwinConstants.DisableRequestCompression) ?? (() => { }); }
        }

        private bool TryParseHostHeader(out IPAddress address, out string host, out int port)
        {
            address = null;
            host = null;
            port = -1;

            var hostHeader = RequestHeaders.GetHeader("Host");
            if (String.IsNullOrWhiteSpace(hostHeader))
            {
                return false;
            }

            // IPv6 (http://www.ietf.org/rfc/rfc2732.txt)
            if (hostHeader.StartsWith("[", StringComparison.Ordinal))
            {
                var portIndex = hostHeader.LastIndexOf("]:", StringComparison.Ordinal);
                if (portIndex != -1 && Int32.TryParse(hostHeader.Substring(portIndex + 2), out port))
                {
                    if (IPAddress.TryParse(hostHeader.Substring(1, portIndex - 1), out address))
                    {
                        host = null;
                        return true;
                    }
                    host = hostHeader.Substring(0, portIndex + 1);
                    return true;
                }
                if (hostHeader.EndsWith("]", StringComparison.Ordinal))
                {
                    if (IPAddress.TryParse(hostHeader.Substring(1, hostHeader.Length - 2), out address))
                    {
                        host = null;
                        port = -1;
                        return true;
                    }
                }
            }
            else
            {
                // IPAddresses
                if (IPAddress.TryParse(hostHeader, out address))
                {
                    host = null;
                    port = -1;
                    return true;
                }

                var portIndex = hostHeader.LastIndexOf(':');
                if (portIndex != -1 && Int32.TryParse(hostHeader.Substring(portIndex + 1), out port))
                {
                    host = hostHeader.Substring(0, portIndex);
                    return true;
                }
            }

            // Plain
            host = hostHeader;
            return true;
        }

        private string RequestHost
        {
            get
            {
                IPAddress address;
                string host;
                int port;
                if (TryParseHostHeader(out address, out host, out port))
                {
                    return host ?? address.ToString();
                }
                return _environment.Get<string>(OwinConstants.LocalIpAddress) ?? IPAddress.Loopback.ToString();
            }
        }

        private int RequestPort
        {
            get
            {
                IPAddress address;
                string host;
                int port;
                if (TryParseHostHeader(out address, out host, out port))
                {
                    if (port == -1)
                    {
                        return DefaultPort;
                    }
                    return port;
                }

                var portString = _environment.Get<string>(OwinConstants.LocalPort);
                if (Int32.TryParse(portString, out port) && port != 0)
                {
                    return port;
                }

                return DefaultPort;
            }
        }

        private int DefaultPort
        {
            get
            {
                return String.Equals(RequestScheme, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
            }
        }

        private string ContentType
        {
            get
            {
                return RequestHeaders.GetHeader("Content-Type");
            }
        }

        private string MediaType
        {
            get
            {
                var contentType = ContentType;
                if (contentType == null)
                {
                    return null;
                }

                var delimiterPos = contentType.IndexOfAny(CommaSemicolon);
                return delimiterPos < 0 ? contentType : contentType.Substring(0, delimiterPos);
            }
        }

        private bool HasFormData
        {
            get
            {
                var mediaType = MediaType;
                return (RequestMethod == "POST" && String.IsNullOrEmpty(mediaType))
                    || mediaType == "application/x-www-form-urlencoded"
                    || mediaType == "multipart/form-data";
            }
        }

        private bool HasParseableData
        {
            get
            {
                var mediaType = MediaType;
                return mediaType == "application/x-www-form-urlencoded"
                    || mediaType == "multipart/form-data";
            }
        }

        private IEnumerable<KeyValuePair<string, string>> ReadForm()
        {
            if (!HasFormData && !HasParseableData)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            var body = RequestBody;
            if (body.CanSeek)
            {
                body.Seek(0, SeekOrigin.Begin);
            }

            var text = new StreamReader(body).ReadToEnd();
            return ParamDictionary.ParseToEnumerable(text);
        }
    }
}
