using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Common.Extensions
{
    public static class XmlExtensions
    {
        public static IEnumerable<XElement> FindDecendants(this XContainer container, string localName)
        {
            return container.Descendants().Where(c => c.Name.LocalName.Equals(localName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool TryGetAttributeValue(this XElement element, string name, out string value)
        {
            var attr = element.Attribute(name);

            if (attr != null)
            {
                value = attr.Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
