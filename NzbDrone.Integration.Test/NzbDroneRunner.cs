using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    public class NzbDroneRunner
    {
        private readonly IProcessProvider _processProvider;
        private readonly IRestClient _restClient;
        private Process _nzbDroneProcess;

        public NzbDroneRunner(int port = 8989)
        {
            _processProvider = new ProcessProvider();
            _restClient = new RestClient("http://localhost:8989/api");
        }


        public void Start()
        {
            AppDate = Path.Combine(Directory.GetCurrentDirectory(), "_intg_" + DateTime.Now.Ticks);

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


                var statusCall = _restClient.Get(new RestRequest("system/status"));

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
            var args = "-nobrowser -data=\"" + AppDate + "\"";
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


        public string AppDate { get; private set; }
    }
}