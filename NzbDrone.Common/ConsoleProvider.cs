using System;
using System.Diagnostics;

namespace NzbDrone.Common
{
    public class ConsoleProvider
    {
        public virtual void WaitForClose()
        {
            while (true)
            {
                Console.ReadLine();
            }
        }

        public virtual void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("     Usage: {0} <command> ", Process.GetCurrentProcess().MainModule.ModuleName);
            Console.WriteLine("     Commands:");
            Console.WriteLine("                 /i  Install the application as a Windows Service ({0}).", ServiceProvider.NzbDroneServiceName);
            Console.WriteLine("                 /u  Uninstall already installed Windows Service ({0}).", ServiceProvider.NzbDroneServiceName);
            Console.WriteLine("                 <No Arguments>  Run application in console mode.");
        }

        public virtual void PrintServiceAlreadyExist()
        {
            Console.WriteLine("A service with the same name ({0}) already exists. Aborting installation", ServiceProvider.NzbDroneServiceName);
        }

        public virtual void PrintServiceDoestExist()
        {
            Console.WriteLine("Can't find service ({0})", ServiceProvider.NzbDroneServiceName);
        }

        public virtual void UpdateFolderDoestExist(string path)
        {
            Console.WriteLine("Unable to find update package in '{0}'", path);
        }
    }
}