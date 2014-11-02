using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.NzbIndex
{
    public class NzbIndexSettingValidator : AbstractValidator<NzbIndexSettings>
    {
        public NzbIndexSettingValidator()
        {
            RuleFor(c => c.Url).ValidRootUrl();
            RuleFor(c => c.QueryParam).NotEmpty();
            RuleFor(c => c.ResponseMaxSize).GreaterThan(0);
            RuleFor(c => c.ResponseMaxSizeParam).NotEmpty();
            RuleFor(c => c.MinSizeParam).NotEmpty();
            RuleFor(c => c.MaxSizeParam).NotEmpty();
            RuleFor(c => c.MaxAgeParam).NotEmpty();
        }
    }

    public class NzbIndexSettings : IProviderConfig
    {
        private static readonly NzbIndexSettingValidator Validator = new NzbIndexSettingValidator();

        public NzbIndexSettings()
        {
            QueryParam = "q";
            ResponseMaxSizeParam = "max";
            MinSizeParam = "minsize";
            MaxSizeParam = "maxsize";
            ResponseMaxSize = 50;
            MaxAgeParam = "age";
            AdditionalParameters = "&complete=1&hidespam=1";
        }

        [FieldDefinition(0, Label = "URL")]
        public String Url { get; set; }
        [FieldDefinition(1, Label = "Query parameter", Advanced = true)]
        public string QueryParam { get; set; }
        [FieldDefinition(2, Label = "Max. size parameter", Advanced = true)]
        public string MaxSizeParam { get; set; }
        [FieldDefinition(3, Label = "Min. size parameter", Advanced = true)]
        public string MinSizeParam { get; set; }
        [FieldDefinition(4, Label = "Max. age parameter", Advanced = true)]
        public string MaxAgeParam { get; set; }
        [FieldDefinition(5, Label = "Response size parameter", Advanced = true)]
        public string ResponseMaxSizeParam { get; set; }
        [FieldDefinition(6, Label = "Response size", Advanced = true, HelpText = "Maximum number of items to request")]
        public int ResponseMaxSize { get; set; }
        [FieldDefinition(7, Label = "Additional Parameters", HelpText = "Additional indexer parameters", Advanced = true)]
        public String AdditionalParameters { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
