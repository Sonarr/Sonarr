using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsRssParser : BasicTorrentRssParser
    {        
        protected override String GetNzbUrl(XElement item)
        {
            var enclosure = item.Element("enclosure");
            return enclosure.Attribute("url").Value;
        }

        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Links().FirstOrDefault();
        }
    }
}