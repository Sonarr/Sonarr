using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public class HadoukenSettingsValidator : AbstractValidator<HadoukenSettings>
    {
        public HadoukenSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.InfinteJWT).NotEmpty().WithMessage("'JWT Token' must not be empty.");
        }
    }

    public class HadoukenSettings : IProviderConfig
    {
        private static readonly HadoukenSettingsValidator Validator = new HadoukenSettingsValidator();

        public HadoukenSettings()
        {
            Host = "localhost";
            Port = 1137;
            Category = "sonarr-tv";
            Preset = "default";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "Porla")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "Porla")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/api")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "InfinteJWT", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string InfinteJWT { get; set; }

        [FieldDefinition(5, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string Category { get; set; }

        [FieldDefinition(6, Label = "Preset", Type = FieldType.Textbox, HelpText = "DownloadClientPorlaSettingsPreset")]
        public string Preset { get; set; }


        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
