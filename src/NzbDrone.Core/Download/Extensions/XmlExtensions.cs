using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NzbDrone.Core.Download.Extensions
{
    internal static class XmlExtensions
    {
        public static string GetStringValue(this XElement element)
        {
            return element.ElementAsString("string");
        }

        public static long GetLongValue(this XElement element)
        {
            return element.ElementAsLong("i8");
        }

        public static int GetIntValue(this XElement element)
        {
            return element.ElementAsInt("i4");
        }

        public static string ElementAsString(this XElement element, XName name, bool trim = false)
        {
            var el = element.Element(name);

            return string.IsNullOrWhiteSpace(el?.Value)
                ? null
                : (trim ? el.Value.Trim() : el.Value);
        }

        public static long ElementAsLong(this XElement element, XName name)
        {
            var el = element.Element(name);
            return long.TryParse(el?.Value, out long value) ? value : default;
        }

        public static int ElementAsInt(this XElement element, XName name)
        {
            var el = element.Element(name);
            return int.TryParse(el?.Value, out int value) ? value : default(int);
        }

        public static int GetIntResponse(this XDocument document)
        {
            return document.XPathSelectElement("./methodResponse/params/param/value").GetIntValue();
        }

        public static string GetStringResponse(this XDocument document)
        {
            return document.XPathSelectElement("./methodResponse/params/param/value").GetStringValue();
        }
    }
}
