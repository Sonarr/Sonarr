//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using CassiniDev.ServerLog;

#endregion

namespace CassiniDev
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
     PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class Server : MarshalByRefObject, IDisposable
    {
        private readonly ApplicationManager _appManager;

        private readonly bool _disableDirectoryListing;

        private readonly string _hostName;

        private readonly IPAddress _ipAddress;

        private readonly object _lockObject;

        private readonly string _physicalPath;

        private readonly int _port;
        private readonly bool _requireAuthentication;
        private readonly int _timeoutInterval;
        private readonly string _virtualPath;
        private bool _disposed;

        private Host _host;

        private IntPtr _processToken;

        private string _processUser;

        private int _requestCount;

        private bool _shutdownInProgress;

        private Socket _socket;

        private Timer _timer;

        public Server(int port, string virtualPath, string physicalPath)
            : this(port, virtualPath, physicalPath, false, false)
        {
        }

        public Server(int port, string physicalPath)
            : this(port, "/", physicalPath, IPAddress.Loopback)
        {
        }

        public Server(string physicalPath)
            : this(CassiniNetworkUtils.GetAvailablePort(32768, 65535, IPAddress.Loopback, false), physicalPath)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName,
                      int timeout, bool requireAuthentication)
            : this(port, virtualPath, physicalPath, ipAddress, hostName, timeout, requireAuthentication, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication)
            : this(port, virtualPath, physicalPath, requireAuthentication, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName)
            : this(port, virtualPath, physicalPath, ipAddress, hostName, 0, false, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName,
                      int timeout, bool requireAuthentication, bool disableDirectoryListing)
            : this(port, virtualPath, physicalPath, requireAuthentication, disableDirectoryListing)
        {
            _ipAddress = ipAddress;
            _hostName = hostName;
            _timeoutInterval = timeout;
        }

        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress)
            : this(port, virtualPath, physicalPath, ipAddress, null, 0, false, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication,
                      bool disableDirectoryListing)
        {
            _ipAddress = IPAddress.Loopback;
            _requireAuthentication = requireAuthentication;
            _disableDirectoryListing = disableDirectoryListing;
            _lockObject = new object();
            _port = port;
            _virtualPath = virtualPath;
            _physicalPath = Path.GetFullPath(physicalPath);
            _physicalPath = _physicalPath.EndsWith("\\", StringComparison.Ordinal)
                                ? _physicalPath
                                : _physicalPath + "\\";
            _appManager = ApplicationManager.GetApplicationManager();
            ObtainProcessToken();
        }

        public Server(string physicalPath, bool requireAuthentication)
            : this(
                CassiniNetworkUtils.GetAvailablePort(32768, 65535, IPAddress.Loopback, false), "/", physicalPath,
                requireAuthentication)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName,
                      int timeout)
            : this(port, virtualPath, physicalPath, ipAddress, hostName, timeout, false, false)
        {
        }

        public bool DisableDirectoryListing
        {
            get { return _disableDirectoryListing; }
        }

        public bool RequireAuthentication
        {
            get { return _requireAuthentication; }
        }

        public int TimeoutInterval
        {
            get { return _timeoutInterval; }
        }

        public string HostName
        {
            get { return _hostName; }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
        }

        public string PhysicalPath
        {
            get { return _physicalPath; }
        }

        public int Port
        {
            get { return _port; }
        }

        public string RootUrl
        {
            get
            {
                string hostname = _hostName;
                if (string.IsNullOrEmpty(_hostName))
                {
                    if (_ipAddress.Equals(IPAddress.Loopback) || _ipAddress.Equals(IPAddress.IPv6Loopback) ||
                        _ipAddress.Equals(IPAddress.Any) || _ipAddress.Equals(IPAddress.IPv6Any))
                    {
                        hostname = "localhost";
                    }
                    else
                    {
                        hostname = _ipAddress.ToString();
                    }
                }

                return _port != 80
                           ?
                               String.Format("http://{0}:{1}{2}", hostname, _port, _virtualPath)
                           :
                               //FIX: #12017 - TODO:TEST
                       string.Format("http://{0}{1}", hostname, _virtualPath);
            }
        }

        public string VirtualPath
        {
            get { return _virtualPath; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_disposed)
            {
                ShutDown();
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        public event EventHandler<RequestEventArgs> RequestComplete;

        public event EventHandler TimedOut;

        public IntPtr GetProcessToken()
        {
            return _processToken;
        }

        public string GetProcessUser()
        {
            return _processUser;
        }

        public void HostStopped()
        {
            _host = null;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }

        // called at the end of request processing
        // to disconnect the remoting proxy for Connection object
        // and allow GC to pick it up
        /// <summary>
        /// </summary>
        /// <param name="conn"></param>
        public void OnRequestEnd(Connection conn)
        {
            try
            {
                OnRequestComplete(conn.Id, conn.RequestLog.Clone(), conn.ResponseLog.Clone());
            }
            catch
            {
                // swallow - we don't want consumer killing the server
            }
            RemotingServices.Disconnect(conn);
            DecrementRequestCount();
        }

        public void Start()
        {
            _socket = CreateSocketBindAndListen(AddressFamily.InterNetwork, _ipAddress, _port);

            //start the timer
            DecrementRequestCount();

            ThreadPool.QueueUserWorkItem(delegate
                {
                    while (!_shutdownInProgress)
                    {
                        try
                        {
                            Socket acceptedSocket = _socket.Accept();

                            ThreadPool.QueueUserWorkItem(delegate
                                {
                                    if (!_shutdownInProgress)
                                    {
                                        Connection conn = new Connection(this, acceptedSocket);

                                        if (conn.WaitForRequestBytes() == 0)
                                        {
                                            conn.WriteErrorAndClose(400);
                                            return;
                                        }

                                        Host host = GetHost();

                                        if (host == null)
                                        {
                                            conn.WriteErrorAndClose(500);
                                            return;
                                        }

                                        IncrementRequestCount();
                                        host.ProcessRequest(conn);
                                    }
                                });
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }
                });
        }


        ~Server()
        {
            Dispose();
        }


        private static Socket CreateSocketBindAndListen(AddressFamily family, IPAddress address, int port)
        {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(address, port));
            socket.Listen((int) SocketOptionName.MaxConnections);
            return socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="hostType"></param>
        /// <returns></returns>
        /// <remarks>This is Dmitry's hack to enable running outside of GAC</remarks>
        private object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType)
        {
            // this creates worker app domain in a way that host doesn't need to be in GAC or bin
            // using BuildManagerHost via private reflection
            string uniqueAppString = string.Concat(virtualPath, physicalPath).ToLowerInvariant();
            string appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

            // create BuildManagerHost in the worker app domain
            //ApplicationManager appManager = ApplicationManager.GetApplicationManager();
            Type buildManagerHostType = typeof (HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            IRegisteredObject buildManagerHost = _appManager.CreateObject(appId, buildManagerHostType, virtualPath,
                                                                          physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember("RegisterAssembly",
                                              BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                                              null,
                                              buildManagerHost,
                                              new object[] {hostType.Assembly.FullName, hostType.Assembly.Location});

            // create Host in the worker app domain
            return _appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
        }

        private void DecrementRequestCount()
        {
            _requestCount--;

            if (_requestCount < 1)
            {
                _requestCount = 0;

                if (_timeoutInterval > 0)
                {
                    _timer = new Timer(TimeOut, null, _timeoutInterval, Timeout.Infinite);
                }
            }
        }

        private Host GetHost()
        {
            if (_shutdownInProgress)
                return null;
            Host host = _host;
            if (host == null)
            {
#if NET40
                object obj2 = new object();
                bool flag = false;
                try
                {
                    Monitor.Enter(obj2 = _lockObject, ref flag);
                    host = _host;
                    if (host == null)
                    {
                        host = (Host)CreateWorkerAppDomainWithHost(_virtualPath, _physicalPath, typeof(Host));
                        host.Configure(this, _port, _virtualPath, _physicalPath, _requireAuthentication, _disableDirectoryListing);
                        _host = host;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        Monitor.Exit(obj2);
                    }
                }
#else

                lock (_lockObject)
                {
                    host = _host;
                    if (host == null)
                    {
                        host = (Host) CreateWorkerAppDomainWithHost(_virtualPath, _physicalPath, typeof (Host));
                        host.Configure(this, _port, _virtualPath, _physicalPath, _requireAuthentication,
                                       _disableDirectoryListing);
                        _host = host;
                    }
                }

#endif
            }

            return host;
        }

        private void IncrementRequestCount()
        {
            _requestCount++;
            _timer = null;
        }


        private void ObtainProcessToken()
        {
            if (Interop.ImpersonateSelf(2))
            {
                Interop.OpenThreadToken(Interop.GetCurrentThread(), 0xf01ff, true, ref _processToken);
                Interop.RevertToSelf();
                // ReSharper disable PossibleNullReferenceException
                _processUser = WindowsIdentity.GetCurrent().Name;
                // ReSharper restore PossibleNullReferenceException
            }
        }

        private void OnRequestComplete(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            EventHandler<RequestEventArgs> complete = RequestComplete;

            if (complete != null)
            {
                complete(this, new RequestEventArgs(id, requestLog, responseLog));
            }
        }


        public void ShutDown()
        {
            _shutdownInProgress = true;

            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                _socket = null;
            }

            try
            {
                if (_host != null)
                {
                    _host.Shutdown();
                }

                while (_host != null)
                {
                    Thread.Sleep(100);
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                _host = null;
            }
        }

        private void TimeOut(object ignored)
        {
            TimeOut();
        }

        public void TimeOut()
        {
            ShutDown();
            OnTimeOut();
        }

        private void OnTimeOut()
        {
            EventHandler handler = TimedOut;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}