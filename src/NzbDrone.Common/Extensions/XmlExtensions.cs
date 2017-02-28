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
    }
}
