using Workarr.Tv;
using Workarr.Validation;

namespace Workarr.AutoTagging.Specifications
{
    public interface IAutoTaggingSpecification
    {
        int Order { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }
        WorkarrValidationResult Validate();

        IAutoTaggingSpecification Clone();
        bool IsSatisfiedBy(Series series);
    }
}
