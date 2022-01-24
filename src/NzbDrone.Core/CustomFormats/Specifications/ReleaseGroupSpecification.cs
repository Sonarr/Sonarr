using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseGroupSpecification : RegexSpecificationBase
    {
        public override int Order => 9;
        public override string ImplementationName => "Release Group";
        public override string InfoLink => "https://wiki.servarr.com/sonarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(ParsedEpisodeInfo episodeInfo)
        {
            return MatchString(episodeInfo?.ReleaseGroup);
        }
    }
}
