using System.ServiceProcess;

namespace NzbDrone
{
    internal class NzbDroneService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}