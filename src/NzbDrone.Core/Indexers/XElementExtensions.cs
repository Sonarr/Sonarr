using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Indexers
{
    public static class XElementExtensions
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(XmlExtensions));

        public static readonly Regex RemoveTimeZoneRegex = new Regex(@"\s[A-Z]{2,4}$", RegexOptions.Compiled);

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
                   (object)root.Value);

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

        public static DateTime ParseDate(string dateString)
        {
            try
            {
                DateTime result;
                if (!DateTime.TryParse(dateString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out result))
                {
                    dateString = RemoveTimeZoneRegex.Replace(dateString, "");
                    result = DateTime.Parse(dateString, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
                }

                return result.ToUniversalTime();
            }
            catch (FormatException e)
            {
                Logger.Warn(e, "Unable to parse " + dateString);
                throw;
            }
        }

        public static DateTime PublishDate(this XElement item)
        {
            var dateString = item.TryGetValue("pubDate");

            return ParseDate(dateString);
        }

        public static List<string> Links(this XElement item)
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

        public static string TryGetValue(this XElement item, string elementName, string defaultValue = "")
        {
            var element = item.Element(elementName);

            return element != null ? element.Value : defaultValue;
        }

        public static T TryGetValue<T>(this XElement item, string elementName, T defaultValue)
        {
            var element = item.Element(elementName);

            if (element == null)
            {
                return defaultValue;
            }

            if (element.Value.IsNullOrWhiteSpace())
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(element.Value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }
    }
}
