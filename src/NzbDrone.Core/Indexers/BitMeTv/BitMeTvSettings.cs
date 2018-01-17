using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
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

            RuleFor(c => c.UiSeedRatio)
                .Must(c => c.IsNullOrWhiteSpace() || double.TryParse(c, out var _))
                .WithMessage("Seed ratio must be a valid decimal number");
        }
    }

    public class BitMeTvSettings : ITorrentIndexerSettings
    {
        private static readonly BitMeTvSettingsValidator Validator = new BitMeTvSettingsValidator();

        public double SeedRatio { get; set; }

        public BitMeTvSettings()
        {
            BaseUrl = "https://www.bitmetv.org";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "UserId")]
        public string UserId { get; set; }

        [FieldDefinition(2, Label = "RSS Passkey")]
        public string RssPasskey { get; set; }

        [FieldDefinition(3, Label = "Cookie", HelpText = "BitMeTv uses a login cookie needed to access the rss, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        [FieldDefinition(4, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5, Type = FieldType.Textbox, Label = "Seed Ratio", HelpText = "The ratio a torrent should reach before stopping, empty is download client's default")]
        public string UiSeedRatio
        {
            get => SeedRatio.ToString();
            set => SeedRatio = double.Parse(value);
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
