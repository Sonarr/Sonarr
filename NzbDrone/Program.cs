using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NLog;
using NzbDrone.Core;

namespace NzbDrone.Console
{
    class Program
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            CentralDispatch.ConfigureNlog();
            Logger.Info("Starting NZBDrone WebUI");
            var server = new CassiniDev.Server(@"D:\My Dropbox\Git\NzbDrone\NzbDrone.Web");
            server.Start();

            System.Diagnostics.Process.Start(server.RootUrl);
            Logger.Info("Server available at: " + server.RootUrl);
            System.Console.ReadLine();

        }
    }
}
