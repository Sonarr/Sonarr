using Workarr.Validation;

namespace Workarr.ThingiProvider
{
    public interface IProviderConfig
    {
        WorkarrValidationResult Validate();
    }
}
