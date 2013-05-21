using System;
using System.Diagnostics;
using System.Reflection;
using NLog;

namespace NzbDrone
{
    public static class AppMain
    {
        private static readonly Logger logger = LogManager.GetLogger("AppMain");


        public static void Main(string[] args)
        {
            try
            {
                logger.Info("Starting NzbDrone Console. Version {0}", Assembly.GetExecutingAssembly().GetName().Version);

                AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e.ExceptionObject as Exception));

                //Check if full version .NET is installed.
                try
                {
                    Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                }
                catch (Exception)
                {
                    logger.Error("It looks like you don't have full version of .NET Framework installed. Press any key and you will be directed to the download page.");
                    Console.Read();

                    try
                    {
                        Process.Start("http://www.microsoft.com/download/en/details.aspx?id=17851");
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Oops. can't start default browser. Please visit http://www.microsoft.com/download/en/details.aspx?id=17851 to download .NET Framework 4.");
                        Console.ReadLine();
                    }

                    return;
                }

                var container = MainAppContainerBuilder.BuildContainer();

                /*try
                {
                    container.Resolve<IUpdateService>().Execute(new ApplicationUpdateCommand());
                }
                catch (Exception e)
                {
                    logger.ErrorException("Application update failed.", e);
                }
*/
                container.Resolve<Router>().Route(args);
            }
            catch (Exception e)
            {
                AppDomainException(e);
            }
        }

        public static void AppDomainException(Exception exception)
        {
            Console.WriteLine("EPIC FAIL: {0}", exception);
            logger.FatalException("EPIC FAIL: " + exception.Message, exception);
        }
    }
}