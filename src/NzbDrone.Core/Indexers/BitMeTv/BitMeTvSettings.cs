using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.BitMeTv
{
    public class BitMeTvSettingsValidator : AbstractValidator<BitMeTvSettings>
    {
        public BitMeTvSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.RssPasskey).NotEmpty();
        }
    }

    public class BitMeTvSettings : IProviderConfig
    {
        private static readonly BitMeTvSettingsValidator validator = new BitMeTvSettingsValidator();

        public BitMeTvSettings()
        {
            BaseUrl = "http://www.bitmetv.org";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        [FieldDefinition(1, Label = "UserId")]
        public String UserId { get; set; }

        [FieldDefinition(2, Label = "RSS Passkey")]
        public String RssPasskey { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}