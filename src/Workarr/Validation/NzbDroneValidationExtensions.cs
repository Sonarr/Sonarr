using FluentValidation;
using FluentValidation.Results;

namespace Workarr.Validation
{
    public static class NzbDroneValidationExtensions
    {
        public static WorkarrValidationResult Filter(this WorkarrValidationResult result, params string[] fields)
        {
            var failures = result.Failures.Where(v => fields.Contains(v.PropertyName)).ToArray();

            return new WorkarrValidationResult(failures);
        }

        public static void ThrowOnError(this WorkarrValidationResult result)
        {
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static bool HasErrors(this List<ValidationFailure> list)
        {
            return list.Any(item => item is not NzbDroneValidationFailure { IsWarning: true });
        }
    }
}
