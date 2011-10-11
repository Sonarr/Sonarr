using System;

namespace NzbDrone.Providers
{
    public class ConsoleProvider
    {
        public virtual void WaitForClose()
        {
            while (true)
            {
                System.Console.ReadLine();
            }
        }

        public virtual void PrintHelp()
        {
            System.Console.WriteLine("Help");
        }
    }
}