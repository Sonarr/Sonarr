using System.Xml.Linq;
using System.Xml.XPath;
using NzbDrone.Core.Download.Extensions;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public class RTorrentFault
    {
        public RTorrentFault(XElement element)
        {
            foreach (var e in element.XPathSelectElements("./value/struct/member"))
            {
                var name = e.ElementAsString("name");
                if (name == "faultCode")
                {
                    FaultCode = e.Element("value").GetIntValue();
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
}
