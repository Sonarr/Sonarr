using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace NzbDrone.Core.Indexers
{
    public static class XElementExtensions
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string Title(this XElement item)
        {
            return item.TryGetValue("title", "Unknown");
        }

        public static DateTime PublishDate(this XElement item)
        {
            string dateString = item.TryGetValue("pubDate");

            try
            {
                return DateTime.Parse(dateString);
            }
            catch (FormatException e)
            {
                Logger.TraceException("Unable to parse " + dateString, e);
                throw;
            }
        }

        public static List<String> Links(this XElement item)
        {
            var elements = item.Elements("link");

            return elements.Select(link => link.Value).ToList();
        }

        public static string Description(this XElement item)
        {
            return item.TryGetValue("description");
        }

        public static string Comments(this XElement item)
        {
            return item.TryGetValue("comments");
        }

        private static string TryGetValue(this XElement item, string elementName, string defaultValue = "")
        {
            var element = item.Element(elementName);

            return element != null ? element.Value : defaultValue;
        }
    }
}
