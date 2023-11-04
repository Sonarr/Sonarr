using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class SubtitleTitleInfo
    {
        private static readonly Regex SubtitleTitleRegex = new Regex("((?<title>.+) - )?(?<copy>\\d+)", RegexOptions.Compiled);
        public List<string> LanguageTags { get; set; }
        public Language Language { get; set; }
        public string RawTitle { get; set; }
        public string Title
        {
            get
            {
                if (RawTitle is null)
                {
                    return null;
                }

                var match = SubtitleTitleRegex.Match(RawTitle);

                if (match.Success)
                {
                    return match.Groups["title"].Success ? match.Groups["title"].ToString() : null;
                }

                return RawTitle;
            }
        }

        public int Copy
        {
            get
            {
                if (RawTitle is null)
                {
                    return 0;
                }

                var match = SubtitleTitleRegex.Match(RawTitle);

                if (match.Success)
                {
                    return int.Parse(match.Groups["copy"].ToString());
                }

                return 0;
            }
        }

        public bool TitleFirst { get; set; }
    }
}
