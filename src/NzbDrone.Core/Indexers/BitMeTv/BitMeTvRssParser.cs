using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.BitMeTv
{
    public class BitMeTvRssParser : BasicTorrentRssParser
    {
        protected override XElement GetTorrentElement(XElement item)
        {
            return item;
        }

        protected override Int64 GetSize(XElement torrentElement)
        {
            return ParseSize(torrentElement.Description(), true);
        }
    }
}