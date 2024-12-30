using FluentValidation.Results;
using Workarr.Extensions;

namespace Workarr.Validation
{
    public class WorkarrValidationResult : ValidationResult
    {
        public WorkarrValidationResult()
        {
            Failures = new List<NzbDroneValidationFailure>();
            Errors = new List<NzbDroneValidationFailure>();
            Warnings = new List<NzbDroneValidationFailure>();
        }

        public WorkarrValidationResult(ValidationResult validationResult)
            : this(validationResult.Errors)
        {
        }

        public WorkarrValidationResult(IEnumerable<ValidationFailure> failures)
        {
            var errors = new List<NzbDroneValidationFailure>();
            var warnings = new List<NzbDroneValidationFailure>();

            foreach (var failureBase in failures)
            {
                if (failureBase is not NzbDroneValidationFailure failure)
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
