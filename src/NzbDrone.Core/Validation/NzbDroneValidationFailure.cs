using System;
using FluentValidation.Results;

namespace NzbDrone.Core.Validation
{
    public class NzbDroneValidationFailure : ValidationFailure
    {
        public String DetailedDescription { get; set; }
        public String InfoLink { get; set; }

        public NzbDroneValidationFailure(String propertyName, String error)
            : base(propertyName, error)
        {

        }
    }
}
