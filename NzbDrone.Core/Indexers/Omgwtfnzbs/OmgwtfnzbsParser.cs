using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsParser : BasicRssParser
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

        protected override ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Description(), @"(?:Size:\<\/b\>\s\d+\.)\d{1,2}\s\w{2}(?:\<br \/\>)", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = GetReportSize(sizeString);
            }

            return currentResult;
        }
    }
}