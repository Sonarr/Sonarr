using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderConfig
    {
        NzbDroneValidationResult Validate();
    }
}
