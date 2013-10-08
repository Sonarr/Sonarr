using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public class NullConfig : IProviderConfig
    {
        public static readonly NullConfig Instance = new NullConfig();

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}