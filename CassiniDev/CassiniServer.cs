// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.Net;


namespace CassiniDev
{
    public class CassiniDevServer
    {

        private Server _server;

        #region Implementation of IDisposable

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            if (_server != null)
            {
                StopServer();
                _server.Dispose();
                _server = null;
            }
        }

        #endregion

        



        /// <summary>
        /// The root URL of the running web application
        /// </summary>
        public string RootUrl
        {
            get { return string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}{2}", _server.HostName, _server.Port, _server.VirtualPath); }

        }
        /// <summary>
        /// Combine the RootUrl of the running web application with the relative url
        /// specified.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public string NormalizeUrl(string relativeUrl)
        {
            return CassiniNetworkUtils.NormalizeUrl(RootUrl, relativeUrl);
        }

        /// <summary>
        /// Will start specified application as "localhost" on loopback and first available port in the range 8000-10000 with vpath "/"
        /// </summary>
        /// <param name="applicationPath">Physical path to application.</param>
        public void StartServer(string applicationPath)
        {
            StartServer(applicationPath, CassiniNetworkUtils.GetAvailablePort(8000, 10000, IPAddress.Loopback, true), "/", "localhost");
        }

        /// <summary>
        /// Will start specified application on loopback
        /// </summary>
        /// <param name="applicationPath">Physical path to application.</param>
        /// <param name="port">Port to listen on.</param>
        /// <param name="virtualPath">Optional. defaults to "/"</param>
        /// <param name="hostName">Optional. Is used to construct RootUrl. Defaults to "localhost"</param>
        public void StartServer(string applicationPath, int port, string virtualPath, string hostName)
        {
            // WebHost.Server will not run on any other IP
            IPAddress ipAddress = IPAddress.Loopback;

            if (!CassiniNetworkUtils.IsPortAvailable(ipAddress, port))
            {
                throw new Exception(string.Format("Port {0} is in use.", port));
            }

            applicationPath = Path.GetFullPath(applicationPath);

            virtualPath = String.Format("/{0}/", (virtualPath ?? string.Empty).Trim('/')).Replace("//", "/");
            hostName = string.IsNullOrEmpty(hostName) ? "localhost" : hostName;

            StartServer(applicationPath, ipAddress, port, virtualPath, hostName);
            

        }

        /// <summary>
        /// </summary>
        /// <param name="applicationPath">Physical path to application.</param>
        /// <param name="ipAddress">IP to listen on.</param>
        /// <param name="port">Port to listen on.</param>
        /// <param name="virtualPath">Optional. default value '/'</param>
        /// <param name="hostname">Optional. Used to construct RootUrl. Defaults to 'localhost'</param>
        public void StartServer(string applicationPath, IPAddress ipAddress, int port, string virtualPath, string hostname)
        {
            if (_server != null)
            {
                throw new InvalidOperationException("Server already started");
            }
            _server = new Server(port, virtualPath, applicationPath, ipAddress,hostname, 60000);

            try
            {
                _server.Start();
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Error starting server instance.", ex);
            }

        }

        /// <summary>
        /// <para>Stops the server.</para>
        /// </summary>
        public void StopServer()
        {
            if (_server != null)
            {
                _server.ShutDown();
            }
        }


    }
}
