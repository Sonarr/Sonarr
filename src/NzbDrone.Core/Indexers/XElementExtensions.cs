using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Indexers
{
    public static class XElementExtensions
    {
        private static readonly Logger Logger =  NzbDroneLogger.GetLogger();

        private static readonly Regex RemoveTimeZoneRegex = new Regex(@"\s[A-Z]{2,4}$", RegexOptions.Compiled);

        public static string Title(this XElement item)
        {
            return item.TryGetValue("title", "Unknown");
        }

        public static XElement StripNameSpace(this XElement root)
        {
            var res = new XElement(
               root.Name.LocalName,
               root.HasElements ?
                   root.Elements().Select(StripNameSpace) :
                   (object)root.Value
           );

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

        public static DateTime PublishDate(this XElement item)
        {
            string dateString = item.TryGetValue("pubDate");

            try
            {
                DateTime result;
                if (!DateTime.TryParse(dateString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out result))
                {
                    dateString = RemoveTimeZoneRegex.Replace(dateString, "");
                    result = DateTime.Parse(dateString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
                }
                return result.ToUniversalTime().Date;
            }
            catch (FormatException e)
            {
                Logger.WarnException("Unable to parse " + dateString, e);
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

        public static long Length(this XElement item)
        {
            return long.Parse(item.TryGetValue("length"));
        }

        private static string TryGetValue(this XElement item, string elementName, string defaultValue = "")
        {
            var element = item.Element(elementName);

            return element != null ? element.Value : defaultValue;
        }
    }
}
