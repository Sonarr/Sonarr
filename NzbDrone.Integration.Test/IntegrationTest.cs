using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NzbDrone.Api.Commands;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Integration.Test.Client;
using NzbDrone.Test.Common.Categories;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    [IntegrationTest]
    public abstract class IntegrationTest
    {
        protected RestClient RestClient { get; private set; }

        protected SeriesClient Series;
        protected ClientBase<RootFolderResource> RootFolders;
        protected ClientBase<CommandResource> Commands;
        protected ReleaseClient Releases;
        protected IndexerClient Indexers;

        [SetUp]
        public void SmokeTestSetup()
        {
            new StartupArguments();

            KillNzbDrone();

            InitRestClients();

            PackageStart();
        }

        private static void KillNzbDrone()
        {
            foreach (var p in Process.GetProcessesByName("NzbDrone.Console"))
            {
                try
                {
                    p.Kill();
                }
                catch (Win32Exception)
                {
                }
            }
        }

        private string AppDate;

        private void PackageStart()
        {
            AppDate = Path.Combine(Directory.GetCurrentDirectory(), "_intg_" + DateTime.Now.Ticks);

            Start("..\\..\\..\\..\\_output\\NzbDrone.Console.exe");

            while (RestClient.Get(new RestRequest("system/status")).ResponseStatus != ResponseStatus.Completed)
            {
                Thread.Sleep(1000);
            }
        }

        private Process Start(string path)
        {

            var args = "-nobrowser -data=\"" + AppDate + "\"";

            var startInfo = new ProcessStartInfo(path, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            Console.WriteLine("Starting {0} {1}", path, args);

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;

                if (eventArgs.Data.Contains("Press enter to exit"))
                {
                    Assert.Fail("Process waiting for input");
                }

                Console.WriteLine(eventArgs.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;

                if (eventArgs.Data.Contains("Press enter to exit"))
                {
                    Assert.Fail("Process waiting for input");
                }

                Console.WriteLine(eventArgs.Data);
            };


            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return process;
        }

        private void InitRestClients()
        {
            RestClient = new RestClient("http://localhost:8989/api");
            Series = new SeriesClient(RestClient);
            Releases = new ReleaseClient(RestClient);
            RootFolders = new ClientBase<RootFolderResource>(RestClient);
            Commands = new ClientBase<CommandResource>(RestClient);
            Indexers = new IndexerClient(RestClient);
        }

        [TearDown]
        public void SmokeTestTearDown()
        {
            KillNzbDrone();
        }
    }

}
