using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new CassiniDev.Server(@"D:\My Dropbox\Git\NzbDrone\NzbDrone.Web");
            server.Start();

            System.Diagnostics.Process.Start(server.RootUrl);
            System.Console.WriteLine(server.RootUrl);
            System.Console.ReadLine();

        }
    }
}
