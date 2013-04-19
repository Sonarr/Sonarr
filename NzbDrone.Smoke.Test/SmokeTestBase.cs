using System;
using System.IO;
using System.Net;
using FluentAssertions;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Nancy.Hosting.Self;
using NzbDrone.Api;
using NzbDrone.Common;
using RestSharp;
using TinyIoC;

namespace NzbDrone.Smoke.Test
{
    [TestFixture]
    public abstract class SmokeTestBase
    {
        private TinyNancyBootstrapper _bootstrapper;
        private NancyHost _host;
        protected RestClient RestClient { get; private set; }

        private static readonly Logger RestLogger = LogManager.GetLogger("REST: ");
        private static readonly Logger Logger = LogManager.GetLogger("TEST: ");
        private EnvironmentProvider _environmentProvider;

        protected TinyIoCContainer Container { get; private set; }


        static SmokeTestBase()
        {
            if (LogManager.Configuration == null || LogManager.Configuration is XmlLoggingConfiguration)
            {
                LogManager.Configuration = new LoggingConfiguration();
                var consoleTarget = new ConsoleTarget { Layout = "${message} ${exception}" };
                LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
            }


            LogManager.ReconfigExistingLoggers();
        }


        [SetUp]
        public void SmokeTestSetup()
        {

            _environmentProvider = new EnvironmentProvider();

            if (Directory.Exists(_environmentProvider.GetAppDataPath()))
            {
                Directory.Delete(_environmentProvider.GetAppDataPath(), true);
            }

            Logger.Info("Working Folder: {0}", _environmentProvider.WorkingDirectory);
            Logger.Info("Data Folder: {0}", _environmentProvider.GetAppDataPath());
            Logger.Info("DB Path: {0}", _environmentProvider.GetNzbDroneDatabase());


            Container = ContainerBuilder.BuildNzbDroneContainer();
            _bootstrapper = new TinyNancyBootstrapper(Container);

            const string url = "http://localhost:1313";

            _host = new NancyHost(new Uri(url), _bootstrapper);

            RestClient = new RestClient(url + "/api/");


            _host.Start();
        }

        [TearDown]
        public void SmokeTestTearDown()
        {
            _host.Stop();
        }


        protected T Get<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.OK) where T : class,new()
        {
            RestLogger.Info("GET: {0}", RestClient.BuildUri(request));

            var response = RestClient.Get<T>(request);

            RestLogger.Info("Response: {0}", response.Content);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            response.ErrorMessage.Should().BeBlank();

            response.StatusCode.Should().Be(statusCode);

            return response.Data;
        }

        protected T Post<T>(IRestRequest request, HttpStatusCode statusCode = HttpStatusCode.Created) where T : class,new()
        {
            RestLogger.Info("POST: {0}", RestClient.BuildUri(request));

            var response = RestClient.Post<T>(request);

            RestLogger.Info("Response: {0}", response.Content);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            response.ErrorMessage.Should().BeBlank();


            response.StatusCode.Should().Be(statusCode);

            return response.Data;
        }

    }
}
