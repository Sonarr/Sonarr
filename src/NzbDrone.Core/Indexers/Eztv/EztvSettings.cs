using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class EztvSettingsValidator : AbstractValidator<EztvSettings>
    {
        public EztvSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class EztvSettings : IProviderConfig
    {
        private static readonly EztvSettingsValidator validator = new EztvSettingsValidator();

        public EztvSettings()
        {
            BaseUrl = "http://www.ezrss.it/";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}