using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace NzbDrone.PostProcessor
{
    internal class Program
    {
        private static string _host = "localhost";
        private static int _port = 8989;
        private static string _apiKey = String.Empty;

        private static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 5)
                {
                    Console.WriteLine("Did this come from SAB? Missing Arguments..");
                    return;
                }

                //Load the ConfigFile
                if (!LoadConfig())
                    return;

                string dir = args[0]; //Get dir from first CMD Line Argument
                string nzbName = args[2]; //Get nzbName from third CMD Line Argument
                string category = args[4]; //Get category from third CMD Line Argument

                var hostString = _host + ":" + _port;

                var url = String.Format("http://{0}/?apiKey={1}&dir={2}&nzbName={3}&category={4}", hostString, _apiKey,
                                        dir, nzbName, category);

                var webClient = new WebClient();
                webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static bool LoadConfig()
        {
            var configFile = "PostProcessor.xml";
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Configuration File does not exist, please create");
                return false;
            }

            var xDoc = XDocument.Load(configFile);
            var config = (from c in xDoc.Descendants("Configuration") select c).FirstOrDefault();

            if (config == null)
            {
                Console.WriteLine("Invalid Configuration File");
                return false;
            }

            var hostNode = config.Descendants("Host").FirstOrDefault();
            var portNode = config.Descendants("Port").FirstOrDefault();
            ;
            var apiKeyNode = config.Descendants("ApiKey").FirstOrDefault();
            ;

            if (hostNode == null || portNode == null || apiKeyNode == null)
            {
                Console.WriteLine("Invalid Configuration File");
                return false;
            }

            _host = hostNode.Value;
            Int32.TryParse(portNode.Value, out _port);
            _apiKey = apiKeyNode.Value;

            return true;
        }
    }
}