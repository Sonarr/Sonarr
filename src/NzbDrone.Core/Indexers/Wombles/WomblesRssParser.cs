using System;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class WomblesRssParser : RssParser
    {
        protected override long GetSize(XElement item)
        {
            // TODO: this can be found in the description element.
            return 0;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue("pubDate") + " +0000";

            return XElementExtensions.ParseDate(dateString);
        }
    }
}