using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class SubtitleTitleInfo
    {
        public List<string> LanguageTags { get; set; }
        public string Title { get; set; }
        public Language Language { get; set; }
    }

    public class SubtitleTitleCopyInfo
    {
        public int Copy { get; set; }
        public string Title { get; set; }

        public SubtitleTitleCopyInfo(int copy, string title)
        {
            Copy = copy;
            Title = title;
        }
    }
}
