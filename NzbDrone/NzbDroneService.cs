using System.ServiceProcess;
using Ninject;

namespace NzbDrone
{
    internal class NzbDroneService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            CentralDispatch.Kernel.Get<ApplicationServer>().Start();
        }

        protected override void OnStop()
        {
            CentralDispatch.Kernel.Get<ApplicationServer>().Stop();
        }
    }
}