using System;
using System.Reflection;
using Ninject;
using NzbDrone.Providers;

namespace NzbDrone
{
    public static class AppMain
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting NzbDrone Console. Version " + Assembly.GetExecutingAssembly().GetName().Version);

                CentralDispatch.Kernel.Get<Router>().Route(args);
            }
            catch (Exception e)
            {
                MonitoringProvider.AppDomainException(e);
            }
        }


    }
}