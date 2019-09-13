using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using RestSharp;

namespace NzbDrone.Test.Common
{
    public class NzbDroneRunner
    {
        private readonly IProcessProvider _processProvider;
        private readonly IRestClient _restClient;
        private Process _nzbDroneProcess;
        private TextWriter _progressWriter;

        public string AppData { get; private set; }
        public string ApiKey { get; private set; }

        public NzbDroneRunner(Logger logger, int port = 8989)
        {
            _processProvider = new ProcessProvider(logger);
            _restClient = new RestClient("http://localhost:8989/api");
        }

        public void Start()
        {
            AppData = Path.Combine(TestContext.CurrentContext.TestDirectory, "_intg_" + TestBase.GetUID());
            Directory.CreateDirectory(AppData);

            GenerateApiKey();
            
            var sonarrConsoleExe = OsInfo.IsWindows ? "Sonarr.Console.exe" : "Sonarr.exe";

            _progressWriter = TestContext.Progress;

            if (BuildInfo.IsDebug)
            {
                Start(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "_output", "Sonarr.Console.exe"));
            }
            else
            {
                Start(Path.Combine("bin", sonarrConsoleExe));
            }

            while (true)
            {
                _nzbDroneProcess.Refresh();

                if (_nzbDroneProcess.HasExited)
                {
                    _progressWriter.WriteLine("NzbDrone has exited unexpectedly");
                    Thread.Sleep(2000);
                    Assert.Fail("Process has exited: ExitCode={0}", _nzbDroneProcess.ExitCode);
                }

                var request = new RestRequest("system/status");
                request.AddHeader("Authorization", ApiKey);
                request.AddHeader("X-Api-Key", ApiKey);

                var statusCall = _restClient.Get(request);

                if (statusCall.ResponseStatus == ResponseStatus.Completed)
                {
                    _progressWriter.WriteLine("NzbDrone is started. Running Tests");
                    return;
                }

                _progressWriter.WriteLine("Waiting for NzbDrone to start. Response Status : {0}  [{1}] {2}", statusCall.ResponseStatus, statusCall.StatusDescription, statusCall.ErrorException.Message);

                Thread.Sleep(500);
            }
        }

        public void KillAll()
        {
            try
            {
                if (_nzbDroneProcess != null)
                {
                    _processProvider.Kill(_nzbDroneProcess.Id);
                }

                _processProvider.KillAll(ProcessProvider.SONARR_CONSOLE_PROCESS_NAME);
                _processProvider.KillAll(ProcessProvider.SONARR_PROCESS_NAME);
            }
            catch (InvalidOperationException)
            {
                // May happen if the process closes while being closed
            }


            TestBase.DeleteTempFolder(AppData);
        }

        private void Start(string outputNzbdroneConsoleExe)
        {
            _progressWriter.WriteLine("Starting instance from {0}", outputNzbdroneConsoleExe);

            var args = "-nobrowser -data=\"" + AppData + "\"";
            _nzbDroneProcess = _processProvider.Start(outputNzbdroneConsoleExe, args, null, OnOutputDataReceived, OnOutputDataReceived);

        }

        private void OnOutputDataReceived(string data)
        {
            _progressWriter.WriteLine(" : " + data);

            if (data.Contains("Press enter to exit"))
            {
                _nzbDroneProcess.StandardInput.WriteLine(" ");
            }
        }

        private void GenerateApiKey()
        {
            var configFile = Path.Combine(AppData, "config.xml");

            // Generate and set the api key so we don't have to poll the config file
            var apiKey = Guid.NewGuid().ToString().Replace("-", "");

            var xDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"), 
                new XElement(ConfigFileProvider.CONFIG_ELEMENT_NAME,
                    new XElement(nameof(ConfigFileProvider.ApiKey), apiKey)
                    )
                );

            var data = xDoc.ToString();

            File.WriteAllText(configFile, data);

            ApiKey = apiKey;
        }
    }
}
