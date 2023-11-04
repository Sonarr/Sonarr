using System;
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
        private Lazy<(string Title, int Copy)> TitleAndCopy => new Lazy<(string title, int copy)>(() =>
        {
            if (RawTitle is null)
            {
                return (null, 0);
            }

            var match = SubtitleTitleRegex.Match(RawTitle);

            if (match.Success)
            {
                return (match.Groups["title"].Success ? match.Groups["title"].ToString() : null, int.Parse(match.Groups["copy"].ToString()));
            }

            return (RawTitle, 0);
        });
        public string Title => TitleAndCopy.Value.Title;
        public int Copy => TitleAndCopy.Value.Copy;
        public bool TitleFirst { get; set; }
    }
}
