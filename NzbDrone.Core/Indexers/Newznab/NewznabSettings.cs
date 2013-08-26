using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        public NewznabSettingsValidator()
        {
            RuleFor(c => c.Url).ValidRootUrl();
        }
    }


    public class NewznabSettings : IIndexerSetting
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            Categories = new[] { 5030, 5040 };
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public IEnumerable<Int32> Categories { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}