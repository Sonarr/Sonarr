using Workarr.Validation;

namespace Workarr.CustomFormats.Specifications
{
    public interface ICustomFormatSpecification
    {
        int Order { get; }
        string InfoLink { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }

        WorkarrValidationResult Validate();

        ICustomFormatSpecification Clone();
        bool IsSatisfiedBy(CustomFormatInput input);
    }
}
