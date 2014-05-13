using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrentsRssParser : BasicTorrentRssParser
    {
        readonly Regex _sizeRegex = new Regex(@"(?<size>\d+(?:\.\d+)? [KMG]i?B)", RegexOptions.Compiled);


        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        protected override Int64 GetSize(XElement torrentElement)
        {
            var description = torrentElement.Element("description").Value;
            var matches = _sizeRegex.Match(description);
            var sizeString = matches.Groups["size"].Value;

            return ParseSize(sizeString, true);
        }
    }
}