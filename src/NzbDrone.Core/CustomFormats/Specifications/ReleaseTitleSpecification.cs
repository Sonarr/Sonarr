using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseTitleSpecification : RegexSpecificationBase
    {
        private static readonly Regex TitleTokenRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

        public override int Order => 1;
        public override string ImplementationName => "Release Title";
        public override string InfoLink => "https://wiki.servarr.com/sonarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(RemoveExcludedTitles(input.EpisodeInfo?.ReleaseTitle, input.ReleaseTitleExclusions)) ||
                   MatchString(RemoveExcludedTitles(input.Filename, input.ReleaseTitleExclusions));
        }

        private static string RemoveExcludedTitles(string title, IEnumerable<string> excludedTitles)
        {
            if (title.IsNullOrWhiteSpace() || excludedTitles == null)
            {
                return title;
            }

            foreach (var excludedTitle in excludedTitles.Where(t => t.IsNotNullOrWhiteSpace()))
            {
                var pattern = GetTitlePattern(excludedTitle);

                if (pattern.IsNotNullOrWhiteSpace())
                {
                    title = Regex.Replace(title, pattern, " ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }

            return title;
        }

        private static string GetTitlePattern(string title)
        {
            var titleTokens = TitleTokenRegex.Matches(title)
                .Cast<Match>()
                .Select(m => Regex.Escape(m.Value))
                .ToList();

            if (titleTokens.Empty())
            {
                return null;
            }

            return $@"(?<![\p{{L}}\p{{N}}]){string.Join(@"[^\p{L}\p{N}]+", titleTokens)}(?![\p{{L}}\p{{N}}])";
        }
    }
}
