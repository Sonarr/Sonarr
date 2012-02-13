using System.Linq;
using System;

namespace ServiceInstall
{
    public static class Program
    {
        static void Main()
        {
            ServiceHelper.Run(@"/i");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}
