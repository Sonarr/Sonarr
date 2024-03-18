using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Porla
{
    // I hate C# constants :(
    // private static readonly string defaultPorlaApiUrl = "/api/v1/jsonrpc";

    public class PorlaSettingsValidator : AbstractValidator<PorlaSettings>
    {
        public PorlaSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.ApiUrl).ValidUrlBase("/api/v1/jsonrpc").When(c => c.UrlBase.IsNotNullOrWhiteSpace());
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.InfinteJWT).NotEmpty().WithMessage("'JWT Token' must not be empty!");
        }
    }

    public class PorlaSettings : IProviderConfig
    {
        private static readonly PorlaSettingsValidator Validator = new PorlaSettingsValidator();

        public PorlaSettings()
        {
            Host = "localhost";
            Port = 1337;
            ApiUrl = "/api/v1/jsonrpc";
            Category = "sonarr-tv";
            Preset = "default";
            TvDirectory = "/tmp";
            SeriesTag = true;
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
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/[apiUrl]")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "ApiUrl", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsApiUrlHelpText")]
        [FieldToken(TokenField.HelpText, "ApiUrl", "clientName", "Porla")]
        [FieldToken(TokenField.HelpText, "ApiUrl", "defaultUrl", "/api/v1/jsonrpc")]
        public string ApiUrl { get; set; }

        [FieldDefinition(5, Label = "InfinteJWT", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string InfinteJWT { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string Category { get; set; }

        [FieldDefinition(7, Label = "Preset", Type = FieldType.Textbox, HelpText = "DownloadClientPorlaSettingsPreset")]
        public string Preset { get; set; }

        [FieldDefinition(8, Label = "Directory", Type = FieldType.Textbox, HelpText = "DownloadClientPorlaSettingsDirectoryHelpText")]
        public string TvDirectory { get; set; }

        [FieldDefinition(9, Label = "DownloadClientPorlaSeriesTag", Type = FieldType.Checkbox, Advanced = true, HelpText = "DownloadClientPorlaSeriesTagHelpText")]
        public bool SeriesTag { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
