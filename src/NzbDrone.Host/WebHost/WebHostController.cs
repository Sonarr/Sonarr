using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Host.AccessControl;
using NzbDrone.Host.Middleware;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace NzbDrone.Host
{
    public class WebHostController : IHostController
    {
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly IEnumerable<IAspNetCoreMiddleware> _middlewares;
        private readonly Logger _logger;
        private IWebHost _host;

        public WebHostController(IRuntimeInfo runtimeInfo,
                                 IConfigFileProvider configFileProvider,
                                 IFirewallAdapter firewallAdapter,
                                 IEnumerable<IAspNetCoreMiddleware> middlewares,
                                 Logger logger)
        {
            _runtimeInfo = runtimeInfo;
            _configFileProvider = configFileProvider;
            _firewallAdapter = firewallAdapter;
            _middlewares = middlewares;
            _logger = logger;
        }

        public void StartServer()
        {
            if (OsInfo.IsWindows)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _firewallAdapter.MakeAccessible();
                }
            }

            var bindAddress = _configFileProvider.BindAddress;
            var enableSsl = _configFileProvider.EnableSsl;
            var sslCertPath = _configFileProvider.SslCertPath;

            var urls = new List<string>();

            urls.Add(BuildUrl("http", bindAddress, _configFileProvider.Port));

            if (enableSsl && sslCertPath.IsNotNullOrWhiteSpace())
            {
                urls.Add(BuildUrl("https", bindAddress, _configFileProvider.SslPort));
            }

            _host = new WebHostBuilder()
                .UseUrls(urls.ToArray())
                .UseKestrel(options =>
                {
                    if (enableSsl && sslCertPath.IsNotNullOrWhiteSpace())
                    {
                        options.ConfigureHttpsDefaults(configureOptions =>
                        {
                            X509Certificate2 certificate;

                            try
                            {
                                certificate = new X509Certificate2(sslCertPath, _configFileProvider.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);
                            }
                            catch (CryptographicException ex)
                            {
                                if (ex.HResult == 0x2 || ex.HResult == 0x2006D080)
                                {
                                    throw new SonarrStartupException(ex, $"The SSL certificate file {sslCertPath} does not exist");
                                }

                                throw new SonarrStartupException(ex);
                            }

                            configureOptions.ServerCertificate = certificate;
                        });
                    }
                })
                .ConfigureKestrel(serverOptions =>
                {
                    serverOptions.AllowSynchronousIO = true;
                    serverOptions.Limits.MaxRequestBodySize = null;
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddProvider(new NLogLoggerProvider());
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .ConfigureServices(services =>
                {
                    services
                    .AddSignalR()
                    .AddNewtonsoftJsonProtocol(options =>
                    {
                        options.PayloadSerializerSettings = Json.GetSerializerSettings();
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.Properties["host.AppName"] = BuildInfo.AppName;
                    app.UsePathBase(_configFileProvider.UrlBase);

                    foreach (var middleWare in _middlewares.OrderBy(c => c.Order))
                    {
                        _logger.Debug("Attaching {0} to host", middleWare.GetType().Name);
                        middleWare.Attach(app);
                    }
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();

            _logger.Info("Listening on the following URLs:");

            foreach (var url in urls)
            {
                _logger.Info("  {0}", url);
            }

            _host.Start();
        }

        public async void StopServer()
        {
            _logger.Info("Attempting to stop OWIN host");

            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
            _host = null;

            _logger.Info("Host has stopped");
        }

        private string BuildUrl(string scheme, string bindAddress, int port)
        {
            return $"{scheme}://{bindAddress}:{port}";
        }
    }
}
