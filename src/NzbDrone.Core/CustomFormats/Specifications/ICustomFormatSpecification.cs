using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatSpecification
    {
        int Order { get; }
        string InfoLink { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }

        ICustomFormatSpecification Clone();
        bool IsSatisfiedBy(ParsedEpisodeInfo episodeInfo);
    }
}
