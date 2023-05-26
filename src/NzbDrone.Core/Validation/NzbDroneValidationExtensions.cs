using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public static class NzbDroneValidationExtensions
    {
        public static NzbDroneValidationResult Filter(this NzbDroneValidationResult result, params string[] fields)
        {
            var failures = result.Failures.Where(v => fields.Contains(v.PropertyName)).ToArray();

            return new NzbDroneValidationResult(failures);
        }

        public static void ThrowOnError(this NzbDroneValidationResult result)
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
