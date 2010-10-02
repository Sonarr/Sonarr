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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace CassiniDev
{

    /// <summary>
    /// Made a go at spinning the server up from this process but after dealing with 
    /// irratic behaviour regarding apartment state, platform concerns, unloaded app domains,
    /// and all the other issues that you can find that people struggle with I just decided
    /// to strictly format the console app's output and just spin up an external process. 
    /// Seems robust so far.
    /// </summary>
    public class CassiniDevServerOP 
    {
        //private bool _disposed;
        
        private string _hostname;
        private StreamWriter _input;
        private IPAddress _ipAddress;
        private Thread _outputThread;
        private string _rootUrl;
        private Process _serverProcess;
        private const int TimeOut = 60000;
        private const int WaitForPort = 5000;


        /// <summary>
        /// </summary>
        public void Dispose()
        {
            {
                if (_serverProcess != null)
                {
                    StopServer();
                }
            }
        }


        /// <summary>
        /// The root URL of the running web application
        /// </summary>
        public string RootUrl
        {
            get { return _rootUrl; }
        }

        /// <summary>
        /// Combine the RootUrl of the running web application with the relative url specified.
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
        /// <param name="hostName">Optional. Used to construct RootUrl. Defaults to 'localhost'</param>
        public virtual void StartServer(string applicationPath, IPAddress ipAddress, int port, string virtualPath, string hostName)
        {
            
            _hostname = hostName;
            _ipAddress = ipAddress;

            // massage and validate arguments
            if (string.IsNullOrEmpty(virtualPath))
            {
                virtualPath = "/";
            }
            if (!virtualPath.StartsWith("/"))
            {
                virtualPath = "/" + virtualPath;
            }
            if (_serverProcess != null)
            {
                throw new InvalidOperationException("Server is running");
            }


            string commandLine = (new CommandLineArguments
            {
                Port = port,
                ApplicationPath = string.Format("\"{0}\"", Path.GetFullPath(applicationPath).Trim('\"').TrimEnd('\\')),
                HostName = hostName,
                IPAddress = ipAddress.ToString(),
                VirtualPath = string.Format("\"{0}\"", virtualPath),
                TimeOut = TimeOut,
                WaitForPort = WaitForPort,
                IPMode = IPMode.Specific,
                PortMode = PortMode.Specific
            }).ToString();


            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
#if NET40 //TODO: find out the real flag
                                  FileName = "CassiniDev4-console.exe",
#else
                                  FileName = "CassiniDev-console.exe",
#endif
      
                    Arguments = commandLine,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };

            // we are going to monitor each line of the output until we get a start or error signal
            // and then just ignore the rest

            string line = null;

            _serverProcess.Start();

            _outputThread = new Thread(() =>
            {
                string l = _serverProcess.StandardOutput.ReadLine();
                while (l != null)
                {
                    if (l.StartsWith("started:") || l.StartsWith("error:"))
                    {
                        line = l;
                    }
                    l = _serverProcess.StandardOutput.ReadLine();
                }
            });
            _outputThread.Start();

            // use StandardInput to send the newline to stop the server when required
            _input = _serverProcess.StandardInput;

            // block until we get a signal
            while (line == null)
            {
                Thread.Sleep(10);
            }

            if (!line.StartsWith("started:"))
            {
                throw new Exception(string.Format("Could not start server: {0}", line));
            }

            // line is the root url
            _rootUrl = line.Substring(line.IndexOf(':') + 1);
        }
        /// <summary>
        /// <para>Stops the server, if running.</para>
        /// </summary>
        public virtual void StopServer()
        {
            StopServer(100);
        }

        /// <summary>
        /// <para>Stops the server, if running.</para>
        /// </summary>
        protected virtual void StopServer(int delay)
        {
            Thread.Sleep(delay);
            if (_serverProcess != null)
            {
                try
                {
                    _input.WriteLine();
                    _serverProcess.WaitForExit(10000);
                    Thread.Sleep(10);
                }
                catch
                {
                }
                finally
                {
                    _serverProcess.Dispose();
                    _serverProcess = null;
                }
            }
        }
    }
}
