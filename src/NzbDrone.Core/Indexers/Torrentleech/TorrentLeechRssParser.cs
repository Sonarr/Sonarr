using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechRssParser : BasicTorrentRssParser
    {
        readonly Regex _descriptionRegex = new Regex(@"Seeders: (?<seeds>\d+) - Leechers: (?<peers>\d+)", RegexOptions.Compiled);

        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        private XElement GetDescription(XElement torrentElement)
        {
            return torrentElement.Element("description");
        }

        protected override Int32? Peers(XElement torrentElement)
        {
            var description = GetDescription(torrentElement).Value;
            var matches = _descriptionRegex.Match(description);
            return Convert.ToInt32(matches.Groups["peers"].Value);
        }

        protected override Int32? Seeders(XElement torrentElement)
        {
            var description = GetDescription(torrentElement).Value;
            var matches = _descriptionRegex.Match(description);
            return Convert.ToInt32(matches.Groups["seeds"].Value);
        }
    }
}