using Workarr.Validation;

namespace Workarr.ThingiProvider
{
    public class NullConfig : IProviderConfig
    {
        public static readonly NullConfig Instance = new NullConfig();

        public WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult();
        }
    }
}
