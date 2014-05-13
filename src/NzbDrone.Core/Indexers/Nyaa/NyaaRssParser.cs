using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaRssParser : BasicTorrentRssParser
    {
        readonly Regex _descriptionRegex = new Regex(@"(?<seeders>\d+) seeder\(s\), (?<peers>\d+) leecher\(s\), (?<downloads>\d+) download\(s\) - (?<size>\d+(?:\.\d+)? [KMG]i?B)", RegexOptions.Compiled);

        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        private XElement GetDescription(XElement torrentElement)
        {
            return torrentElement.Element("description");
        }

        protected override Int64 GetSize(XElement item)
        {
            var description = GetDescription(item).Value;
            var matches = _descriptionRegex.Match(description);

            var sizeString = matches.Groups["size"].Value;

            Double sizeValue = Double.Parse(sizeString.Split(' ').First(), CultureInfo.InvariantCulture);
            Int64 fileSize = 0;

            if (sizeString.IndexOf("G") != -1)
            {
                fileSize = Convert.ToInt64(sizeValue * 1073741824);
            }
            else if (sizeString.IndexOf("M") != -1)
            {
                fileSize = Convert.ToInt64(sizeValue * 1048576);
            }
            else if (sizeString.IndexOf("K") != -1) // KB ??? need to find examples
            {
                fileSize = Convert.ToInt64(sizeValue * 1024);
            }
            else // bytes? ??? need to find examples
            {
                fileSize = Convert.ToInt64(sizeValue);
            }

            return fileSize;
        }

        protected override int? Seeders(XElement torrentElement)
        {
            var description = GetDescription(torrentElement).Value;
            var matches = _descriptionRegex.Match(description);

            var seeders = Convert.ToInt32(matches.Groups["seeders"].Value);
            var peers = Convert.ToInt32(matches.Groups["peers"].Value);
            
            if (seeders == 0 && peers == 0)
            {
                return null;
            }

            return seeders;
        }

        protected override int? Peers(XElement torrentElement)
        {
            var description = GetDescription(torrentElement).Value;
            var matches = _descriptionRegex.Match(description);

            var seeders = Convert.ToInt32(matches.Groups["seeders"].Value);
            var peers = Convert.ToInt32(matches.Groups["peers"].Value);

            if (seeders == 0 && peers == 0)
            {
                return null;
            }

            return peers;
        }
    }
}