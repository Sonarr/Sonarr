using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationResult : ValidationResult
    {
        public NzbDroneValidationResult()
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

        public virtual bool HasWarnings
        {
            get { return Warnings.Any(); }
        }
    }
}