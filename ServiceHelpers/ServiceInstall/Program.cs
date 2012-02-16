using System.Linq;
using System;

namespace ServiceInstall
{
    public static class Program
    {
        static void Main()
        {
            ServiceHelper.Run(@"/i");
        }
    }
}
