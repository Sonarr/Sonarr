using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationResult : ValidationResult
    {
        public NzbDroneValidationResult()
        {
            Failures = new List<NzbDroneValidationFailure>();
            Errors = new List<NzbDroneValidationFailure>();
            Warnings = new List<NzbDroneValidationFailure>();
        }

        public NzbDroneValidationResult(ValidationResult validationResult)
            : this(validationResult.Errors)
        {
        }

        public NzbDroneValidationResult(IEnumerable<ValidationFailure> failures)
        {
            var errors = new List<NzbDroneValidationFailure>();
            var warnings = new List<NzbDroneValidationFailure>();

            foreach (var failureBase in failures)
            {
                var failure = failureBase as NzbDroneValidationFailure;
                if (failure == null)
                {
                    failure = new NzbDroneValidationFailure(failureBase);
                }

                if (failure.IsWarning)
                {
                    warnings.Add(failure);
                }
                else
                {
                    errors.Add(failure);
                }
            }

            Failures = errors.Concat(warnings).ToList();
            Errors = errors;
            errors.ForEach(base.Errors.Add);
            Warnings = warnings;
        }

        public IList<NzbDroneValidationFailure> Failures { get; private set; }
        public new IList<NzbDroneValidationFailure> Errors { get; private set; }
        public IList<NzbDroneValidationFailure> Warnings { get; private set; }

        public virtual bool HasWarnings => Warnings.Any();

        public override bool IsValid => Errors.Empty();
    }
}
