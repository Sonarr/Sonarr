using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public interface IAutoTaggingSpecification
    {
        int Order { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }
        NzbDroneValidationResult Validate();

        IAutoTaggingSpecification Clone();
        bool IsSatisfiedBy(Series series);
    }
}
