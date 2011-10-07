using System;

namespace NzbDrone.Providers
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
    }
}
