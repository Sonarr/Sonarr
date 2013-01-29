using System;
using System.Diagnostics;
using System.Threading;

namespace NzbDrone.Common
{
    public class ConsoleProvider
    {
        public virtual void WaitForClose()
        {
            while (true)
            {
                Console.ReadLine();
                Thread.Sleep(250);
            }
        }

        public virtual void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("     Usage: {0} <command> ", Process.GetCurrentProcess().MainModule.ModuleName);
            Console.WriteLine("     Commands:");
            Console.WriteLine("                 /i  Install the application as a Windows Service ({0}).", ServiceProvider.NZBDRONE_SERVICE_NAME);
            Console.WriteLine("                 /u  Uninstall already installed Windows Service ({0}).", ServiceProvider.NZBDRONE_SERVICE_NAME);
            Console.WriteLine("                 <No Arguments>  Run application in console mode.");
        }

        public virtual void PrintServiceAlreadyExist()
        {
            Console.WriteLine("A service with the same name ({0}) already exists. Aborting installation", ServiceProvider.NZBDRONE_SERVICE_NAME);
        }

        public virtual void PrintServiceDoestExist()
        {
            Console.WriteLine("Can't find service ({0})", ServiceProvider.NZBDRONE_SERVICE_NAME);
        }
    }
}