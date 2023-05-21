using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationFailure : ValidationFailure
    {
        public bool IsWarning { get; set; }
        public string DetailedDescription { get; set; }
        public string InfoLink { get; set; }

        public NzbDroneValidationFailure(string propertyName, string error)
            : base(propertyName, error)
        {
        }

        public NzbDroneValidationFailure(string propertyName, string error, object attemptedValue)
            : base(propertyName, error, attemptedValue)
        {
        }

        public NzbDroneValidationFailure(ValidationFailure validationFailure)
            : base(validationFailure.PropertyName, validationFailure.ErrorMessage, validationFailure.AttemptedValue)
        {
            CustomState = validationFailure.CustomState;
            var state = validationFailure.CustomState as NzbDroneValidationState;

            IsWarning = state is { IsWarning: true };
        }
    }
}
