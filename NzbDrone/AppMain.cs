using System;
using System.Reflection;
using NLog;
using Ninject;

namespace NzbDrone
{
    public static class AppMain
    {


        private static readonly Logger Logger = LogManager.GetLogger("Host.Main");

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting NzbDrone Console. Version " + Assembly.GetExecutingAssembly().GetName().Version);

                CentralDispatch.Kernel.Get<Router>().Route(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Fatal(e.ToString());
            }
        }


    }
}