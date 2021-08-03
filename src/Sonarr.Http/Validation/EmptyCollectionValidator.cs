using System.Collections.Generic;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Validation
{
    public class EmptyCollectionValidator<T> : PropertyValidator
    {
        public EmptyCollectionValidator()
            : base("Collection Must Be Empty")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return context.PropertyValue is IEnumerable<T> collection && collection.Empty();
        }
    }
}
