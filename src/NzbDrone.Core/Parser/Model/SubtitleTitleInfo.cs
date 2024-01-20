using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class SubtitleTitleInfo
    {
        public List<string> LanguageTags { get; set; }
        public Language Language { get; set; }
        public string RawTitle { get; set; }
        public string Title { get; set; }
        public int Copy { get; set; }
        public bool TitleFirst { get; set; }
    }
}
