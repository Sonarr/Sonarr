using System.Collections.Generic;
using System.Text;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public SubtitleFile()
        {
            LanguageTags = new List<string>();
        }

        public Language Language { get; set; }

        public string AggregateString => Language + Title + LanguageTagsAsString + Extension;

        public int Copy { get; set; }

        public List<string> LanguageTags { get; set; }

        public string Title { get; set; }

        private string LanguageTagsAsString => string.Join(".", LanguageTags);

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("[{0}] ", Id);
            stringBuilder.Append(RelativePath);

            stringBuilder.Append(" (");
            stringBuilder.Append(Language);
            if (Title is not null)
            {
                stringBuilder.Append('.');
                stringBuilder.Append(Title);
            }

            if (LanguageTags.Count > 0)
            {
                stringBuilder.Append('.');
                stringBuilder.Append(LanguageTagsAsString);
            }

            stringBuilder.Append(Extension);
            stringBuilder.Append(')');

            return stringBuilder.ToString();
        }
    }
}
