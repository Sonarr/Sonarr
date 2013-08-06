using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers
{
    public static class XElementExtensions
    {
        public static string Title(this XElement item)
        {
            return TryGetValue(item, "title", "Unknown");
        }

        public static DateTime PublishDate(this XElement item)
        {
            return DateTime.Parse(TryGetValue(item, "pubDate"));
        }

        public static List<String> Links(this XElement item)
        {
            var result = new List<String>();
            var elements = item.Elements("link");

            foreach (var link in elements)
            {
                result.Add(link.Value);
            }

            return result;
        }

        public static string Description(this XElement item)
        {
            return TryGetValue(item, "description");
        }

        public static string Comments(this XElement item)
        {
            return TryGetValue(item, "comments");
        }

        private static string TryGetValue(XElement item, string elementName, string defaultValue = "")
        {
            var element = item.Element(elementName);

            return element != null ? element.Value : defaultValue;
        }
    }
}
