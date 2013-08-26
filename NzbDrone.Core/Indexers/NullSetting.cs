using FluentValidation.Results;

namespace NzbDrone.Core.Indexers
{
    public class NullSetting : IIndexerSetting
    {
        public static readonly NullSetting Instance = new NullSetting();

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}