using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NzbDrone.Test.Dummy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dummy process. ID:{0}  Path:{1}", Process.GetCurrentProcess().Id, Process.GetCurrentProcess().MainModule.FileName);
            Console.ReadLine();
        }
    }
}
