using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public abstract class NzbDroneValidator<T> : AbstractValidator<T>
    {
        public override ValidationResult Validate(T instance)
        {
            return new NzbDroneValidationResult(base.Validate(instance).Errors);
        }
    }
}
