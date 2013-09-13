using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NzbDrone.Core.Indexers.NzbClub
{
    public class NzbClubParser : RssParserBase
    {

        private static readonly Regex SizeRegex = new Regex(@"(?:Size:)\s(?<size>\d+.\d+\s[g|m]i?[b])", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        protected override long GetSize(XElement item)
        {
            var match = SizeRegex.Match(item.Description());

            if (match.Success && match.Groups["size"].Success)
            {
                return ParseSize(match.Groups["size"].Value);
            }

            return 0;
        }

        protected override string GetTitle(XElement item)
        {
            var title = ParseHeader(item.Title());

            if (String.IsNullOrWhiteSpace(title))
                return item.Title();

            return title;
        }

        private static readonly Regex[] HeaderRegex = new[]
                                                          {
                                                                new Regex(@"(?:\[.+\]\-\[.+\]\-\[.+\]\-\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                        RegexOptions.IgnoreCase),
                                                                
                                                                new Regex(@"(?:\[.+\]\W+\[.+\]\W+\[.+\]\W+\"")(?<nzbTitle>.+)(?:\"".+)",
                                                                        RegexOptions.IgnoreCase),
                                                                    
                                                                new Regex(@"(?:\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                        RegexOptions.IgnoreCase),
                                                          };

        private static string ParseHeader(string header)
        {
            foreach (var regex in HeaderRegex)
            {
                var match = regex.Matches(header);

                if (match.Count != 0)
                    return match[0].Groups["nzbTitle"].Value.Trim();
            }

            return header;
        }

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Links().First();
        }

        protected override string GetNzbUrl(XElement item)
        {
            var enclosure = item.Element("enclosure");

            return enclosure.Attribute("url").Value;
        }
    }
}
