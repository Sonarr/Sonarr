using FluentValidation.Results;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerSetting
    {
        ValidationResult Validate();
    }
}
