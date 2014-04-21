using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubParser : RssParserBase
    {
        protected override string GetNzbInfoUrl(XElement item)
        {
            IEnumerable<XElement> matches = item.DescendantsAndSelf("link");
            if (matches.Any())
            {
                return matches.First().Value;
            }
            return String.Empty;
        }

        protected override long GetSize(XElement item)
        {
            IEnumerable<XElement> matches = item.DescendantsAndSelf("enclosure");
            if (matches.Any())
            {
                XElement enclosureElement = matches.First();
                return Convert.ToInt64(enclosureElement.Attribute("length").Value);
            }
            return 0;
        }
    }
}
