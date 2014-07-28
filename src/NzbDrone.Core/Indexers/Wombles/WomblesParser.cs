using System;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class WomblesParser : RssParserBase
    {
        protected override string GetNzbInfoUrl(XElement item)
        {
            return null;
        }

        protected override long GetSize(XElement item)
        {
            return 0;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue("pubDate") + " +0000";

            return XElementExtensions.ParseDate(dateString);
        }
    }
}