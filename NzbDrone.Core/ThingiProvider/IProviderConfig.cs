using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderConfig
    {
        ValidationResult Validate();
    }
}