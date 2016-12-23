using System.Text.RegularExpressions;
using FluentValidation;
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

            RuleFor(c => c.Cookie).NotEmpty();

            RuleFor(c => c.Cookie)
                .Matches(@"pass=[0-9a-f]{32}", RegexOptions.IgnoreCase)
                .WithMessage("Wrong pattern")
                .AsWarning();
        }
    }

    public class BitMeTvSettings : IProviderConfig
    {
        private static readonly BitMeTvSettingsValidator Validator = new BitMeTvSettingsValidator();

        public BitMeTvSettings()
        {
            BaseUrl = "https://www.bitmetv.org";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "UserId")]
        public string UserId { get; set; }

        [FieldDefinition(2, Label = "RSS Passkey")]
        public string RssPasskey { get; set; }

        [FieldDefinition(3, Label = "Cookie", HelpText = "BitMeTv uses a login cookie needed to access the rss, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}