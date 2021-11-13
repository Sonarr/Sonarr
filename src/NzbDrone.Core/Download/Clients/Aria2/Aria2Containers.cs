using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NzbDrone.Core.Download.Extensions;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2Fault
    {
        public Aria2Fault(XElement element)
        {
            foreach (var e in element.XPathSelectElements("./value/struct/member"))
            {
                var name = e.ElementAsString("name");
                if (name == "faultCode")
                {
                    FaultCode = e.Element("value").ElementAsInt("int");
                }
                else if (name == "faultString")
                {
                    FaultString = e.Element("value").GetStringValue();
                }
            }
        }

        public int FaultCode { get; set; }
        public string FaultString { get; set; }
    }

    public class Aria2Version
    {
        public Aria2Version(XElement element)
        {
            foreach (var e in element.XPathSelectElements("./struct/member"))
            {
                if (e.ElementAsString("name") == "version")
                {
                    Version = e.Element("value").GetStringValue();
                }
            }
        }

        public string Version { get; set; }
    }

    public class Aria2File
    {
        public Aria2File(XElement element)
        {
            foreach (var e in element.XPathSelectElements("./struct/member"))
            {
                var name = e.ElementAsString("name");

                if (name == "path")
                {
                    Path = e.Element("value").GetStringValue();
                }
            }
        }

        public string Path { get; set; }
    }

    public class Aria2Dict
    {
        public Aria2Dict(XElement element)
        {
            Dict = new Dictionary<string, string>();

            foreach (var e in element.XPathSelectElements("./struct/member"))
            {
                Dict.Add(e.ElementAsString("name"), e.Element("value").GetStringValue());
            }
        }

        public Dictionary<string, string> Dict { get; set; }
    }

    public class Aria2Bittorrent
    {
        public Aria2Bittorrent(XElement element)
        {
            foreach (var e in element.Descendants("member"))
            {
                if (e.ElementAsString("name") == "name")
                {
                    Name = e.Element("value").GetStringValue();
                }
            }
        }

        public string Name;
    }

    public class Aria2Status
    {
        public Aria2Status(XElement element)
        {
            foreach (var e in element.XPathSelectElements("./struct/member"))
            {
                var name = e.ElementAsString("name");

                if (name == "bittorrent")
                {
                    Bittorrent = new Aria2Bittorrent(e.Element("value"));
                }
                else if (name == "infoHash")
                {
                    InfoHash = e.Element("value").GetStringValue();
                }
                else if (name == "completedLength")
                {
                    CompletedLength = e.Element("value").GetStringValue();
                }
                else if (name == "downloadSpeed")
                {
                    DownloadSpeed = e.Element("value").GetStringValue();
                }
                else if (name == "files")
                {
                    Files = e.XPathSelectElement("./value/array/data")
                        .Elements()
                        .Select(x => new Aria2File(x))
                        .ToArray();
                }
                else if (name == "gid")
                {
                    Gid = e.Element("value").GetStringValue();
                }
                else if (name == "status")
                {
                    Status = e.Element("value").GetStringValue();
                }
                else if (name == "totalLength")
                {
                    TotalLength = e.Element("value").GetStringValue();
                }
                else if (name == "uploadLength")
                {
                    UploadLength = e.Element("value").GetStringValue();
                }
                else if (name == "errorMessage")
                {
                    ErrorMessage = e.Element("value").GetStringValue();
                }
            }
        }

        public Aria2Bittorrent Bittorrent { get; set; }
        public string InfoHash { get; set; }
        public string CompletedLength { get; set; }
        public string DownloadSpeed { get; set; }
        public Aria2File[] Files { get; set; }
        public string Gid { get; set; }
        public string Status { get; set; }
        public string TotalLength { get; set; }
        public string UploadLength { get; set; }
        public string ErrorMessage { get; set; }
    }
}
