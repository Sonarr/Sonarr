using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseTitleSpecification : RegexSpecificationBase
    {
        public override int Order => 1;
        public override string ImplementationName => "Release Title";
        public override string InfoLink => "https://wiki.servarr.com/sonarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(ParsedEpisodeInfo episodeInfo)
        {
            var filename = (string)episodeInfo?.ExtraInfo?.GetValueOrDefault("Filename");

            return MatchString(episodeInfo?.ReleaseTitle) || MatchString(filename);
        }
    }
}
