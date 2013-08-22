using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettings : IIndexerSetting
    {
        public NewznabSettings()
        {
            Categories = new[] { 5030, 5040 };
            //RuleFor(c => c.Url).ValidRootUrl();
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public IEnumerable<Int32> Categories { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Url);
            }
        }

        public ValidationResult Validate()
        {
            return new ValidationResult();
            //return Validate(this);
        }
    }
}