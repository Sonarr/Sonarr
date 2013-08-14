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

            if (BuildInfo.IsDebug)
            {

                Start("..\\..\\..\\..\\_output\\NzbDrone.Console.exe");
            }
            else
            {
                Start("bin\\NzbDrone.Console.exe");
            }

            while (true)
            {
                _nzbDroneProcess.Refresh();

                if (_nzbDroneProcess.HasExited)
                {
                    Assert.Fail("Process has exited");
                }

                if (_restClient.Get(new RestRequest("system/status")).ResponseStatus == ResponseStatus.Completed)
                {
                    return;
                }

                Thread.Sleep(500);
            }
        }

        public void KillAll()
        {
            _processProvider.KillAll(ProcessProvider.NzbDroneConsoleProcessName);
            _processProvider.KillAll(ProcessProvider.NzbDroneProcessName);
        }

        private void Start(string outputNzbdroneConsoleExe)
        {
            var args = "-nobrowser -data=\"" + AppDate + "\"";
            _nzbDroneProcess = _processProvider.ShellExecute(outputNzbdroneConsoleExe, args, OnOutputDataReceived, OnOutputDataReceived);

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