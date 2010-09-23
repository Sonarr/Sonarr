using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new CassiniDev.CassiniDevServer();
            server.StartServer(@"D:\Opensource\Spongebob\NzbDrone.Web");


            System.Console.WriteLine(server.NormalizeUrl(@"series"));
            System.Console.ReadLine();

        }
    }
}
