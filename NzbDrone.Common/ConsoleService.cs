using System;
using System.Diagnostics;
using System.IO;

namespace NzbDrone.Common
{
    public interface IConsoleService
    {
        bool IsConsoleApplication { get; }
        void WaitForClose();
        void PrintHelp();
        void PrintServiceAlreadyExist();
        void PrintServiceDoestExist();
    }

    public class ConsoleService : IConsoleService
    {
        public bool IsConsoleApplication
        {
            get { return Console.In != StreamReader.Null; }
        }

        public void WaitForClose()
        {
            while (true)
            {
                Console.ReadLine();
            }
        }

        public void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("     Usage: {0} <command> ", Process.GetCurrentProcess().MainModule.ModuleName);
            Console.WriteLine("     Commands:");
            Console.WriteLine("                 /i  Install the application as a Windows Service ({0}).", ServiceProvider.NZBDRONE_SERVICE_NAME);
            Console.WriteLine("                 /u  Uninstall already installed Windows Service ({0}).", ServiceProvider.NZBDRONE_SERVICE_NAME);
            Console.WriteLine("                 <No Arguments>  Run application in console mode.");
        }

        public void PrintServiceAlreadyExist()
        {
            Console.WriteLine("A service with the same name ({0}) already exists. Aborting installation", ServiceProvider.NZBDRONE_SERVICE_NAME);
        }

        public void PrintServiceDoestExist()
        {
            Console.WriteLine("Can't find service ({0})", ServiceProvider.NZBDRONE_SERVICE_NAME);
        }
    }
}