using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using NLog;
using Ninject;

namespace NzbDrone
{
    public class NzbDroneService : ServiceBase
    {

        private static readonly Logger Logger = LogManager.GetLogger("Host.CentralDispatch");

        protected override void OnStart(string[] args)
        {
            try
            {
                while (!Debugger.IsAttached) Thread.Sleep(100);
                Debugger.Break();
                CentralDispatch.Kernel.Get<ApplicationServer>().Start();
            }
            catch (Exception e)
            {

                Logger.Fatal("Failed to start Windows Service", e);
            }

        }

        protected override void OnStop()
        {
            try
            {
                CentralDispatch.Kernel.Get<ApplicationServer>().Stop();
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to stop Windows Service", e);
            }
        }
    }
}