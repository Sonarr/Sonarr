using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    public class NzbDroneRunner
    {
        private readonly IProcessProvider _processProvider;
        private readonly IRestClient _restClient;
        private Process _nzbDroneProcess;

        public string AppData { get; private set; }
        public string ApiKey { get; private set; }

        public NzbDroneRunner(int port = 8989)
        {
            _processProvider = new ProcessProvider();
            _restClient = new RestClient("http://localhost:8989/api");
        }

        public void Start()
        {
            AppData = Path.Combine(Directory.GetCurrentDirectory(), "_intg_" + DateTime.Now.Ticks);

            var nzbdroneConsoleExe = "NzbDrone.Console.exe";

            if (OsInfo.IsMono)
            {
                nzbdroneConsoleExe = "NzbDrone.exe";
            }

            if (BuildInfo.IsDebug)
            {

                Start("..\\..\\..\\..\\_output\\NzbDrone.Console.exe");
            }
            else
            {
                Start(Path.Combine("bin", nzbdroneConsoleExe));
            }

            while (true)
            {
                _nzbDroneProcess.Refresh();

                if (_nzbDroneProcess.HasExited)
                {
                    Assert.Fail("Process has exited");
                }

                SetApiKey();

                var request = new RestRequest("system/status");
                request.AddHeader("Authorization", ApiKey);

                var statusCall = _restClient.Get(request);

                if (statusCall.ResponseStatus == ResponseStatus.Completed)
                {
                    Console.WriteLine("NzbDrone is started. Running Tests");
                    return;
                }

                Console.WriteLine("Waiting for NzbDrone to start. Response Status : {0}  [{1}] {2}", statusCall.ResponseStatus, statusCall.StatusDescription, statusCall.ErrorException);

                Thread.Sleep(500);
            }
        }

        public void KillAll()
        {
            _processProvider.KillAll(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME);
            _processProvider.KillAll(ProcessProvider.NZB_DRONE_PROCESS_NAME);
        }

        private void Start(string outputNzbdroneConsoleExe)
        {
            var args = "-nobrowser -data=\"" + AppData + "\"";
            _nzbDroneProcess = _processProvider.Start(outputNzbdroneConsoleExe, args, OnOutputDataReceived, OnOutputDataReceived);

        }

        private void OnOutputDataReceived(string data)
        {
            Console.WriteLine(data);

            if (data.Contains("Press enter to exit"))
            {
                _nzbDroneProcess.StandardInput.WriteLine(" ");
            }
        }

        private void SetApiKey()
        {
            var configFile = Path.Combine(AppData, "config.xml");

            if (!String.IsNullOrWhiteSpace(ApiKey)) return;
            if (!File.Exists(configFile)) return;

            var xDoc = XDocument.Load(configFile);
            var config = xDoc.Descendants(ConfigFileProvider.CONFIG_ELEMENT_NAME).Single();
            ApiKey = config.Descendants("ApiKey").Single().Value;
        }
    }
}