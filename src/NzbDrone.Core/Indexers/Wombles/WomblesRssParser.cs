using System;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class WomblesRssParser : RssParser
    {
        public WomblesRssParser()
        {
            ParseSizeInDescription = true;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue("pubDate") + " +0000";

            return XElementExtensions.ParseDate(dateString);
        }
    }
}