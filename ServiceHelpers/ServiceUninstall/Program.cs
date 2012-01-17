using System.Linq;
using System;
using UninstallService;

namespace ServiceUninstall
{
    public static class Program
    {
        static void Main()
        {
            ServiceHelper.Run(@"/u");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}
