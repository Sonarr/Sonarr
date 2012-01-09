using System;
using System.Diagnostics;
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

                //Check if full version .NET is installed.
                try
                {
                    //var webAssembly = Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                }
                catch (Exception e)
                {
                    Console.WriteLine("It seems like you don't have full version of .NET framework installed. Press any key and you will be directed to download page.");
                    Console.Read();

                    try
                    {
                        Process.Start("http://www.microsoft.com/download/en/details.aspx?id=17851");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Was unable to start default browser. Please visit http://www.microsoft.com/download/en/details.aspx?id=17851 to download .NET framework 4.");
                        Console.ReadLine();
                    }

                    return;
                }


                CentralDispatch.Kernel.Get<Router>().Route(args);
            }
            catch (Exception e)
            {
                MonitoringProvider.AppDomainException(e);
            }
        }


    }
}