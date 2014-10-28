using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsRssParser : RssParser
    {
        public OmgwtfnzbsRssParser()
        {
            UseEnclosureUrl = true;
            UseEnclosureLength = true;
        }

        protected override string GetInfoUrl(XElement item)
        {
            //Todo: Me thinks I need to parse details to get this...
            var match = Regex.Match(item.Description(), @"(?:\<b\>View NZB\:\<\/b\>\s\<a\shref\=\"")(?<URL>.+)(?:\""\starget)",
                                    RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success)
            {
                return match.Groups["URL"].Value;
            }

            return String.Empty;
        }
    }
}