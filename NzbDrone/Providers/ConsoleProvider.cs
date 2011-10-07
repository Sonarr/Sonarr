using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
