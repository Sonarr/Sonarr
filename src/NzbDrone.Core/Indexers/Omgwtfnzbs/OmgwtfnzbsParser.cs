using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsParser : RssParserBase
    {
        protected override string GetNzbInfoUrl(XElement item)
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

        protected override long GetSize(XElement item)
        {
            var sizeString = Regex.Match(item.Description(), @"(?:Size:\<\/b\>\s\d+\.)\d{1,2}\s\w{2}(?:\<br \/\>)", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
            return ParseSize(sizeString);
        }
    }
}