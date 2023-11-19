using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.FreeboxDownload
{
    public class FreeboxDownloadSettingsValidator : AbstractValidator<FreeboxDownloadSettings>
    {
        public FreeboxDownloadSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.ApiUrl).NotEmpty()
                                  .WithMessage("'API URL' must not be empty.");
            RuleFor(c => c.ApiUrl).ValidUrlBase();
            RuleFor(c => c.AppId).NotEmpty()
                                 .WithMessage("'App ID' must not be empty.");
            RuleFor(c => c.AppToken).NotEmpty()
                                    .WithMessage("'App Token' must not be empty.");
            RuleFor(c => c.Category).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase)
                                    .WithMessage("Allowed characters a-z and -");
            RuleFor(c => c.DestinationDirectory).IsValidPath()
                                                .When(c => c.DestinationDirectory.IsNotNullOrWhiteSpace());
            RuleFor(c => c.DestinationDirectory).Empty()
                                    .When(c => c.Category.IsNotNullOrWhiteSpace())
                                    .WithMessage("Cannot use 'Category' and 'Destination Directory' at the same time.");
            RuleFor(c => c.Category).Empty()
                                    .When(c => c.DestinationDirectory.IsNotNullOrWhiteSpace())
                                    .WithMessage("Cannot use 'Category' and 'Destination Directory' at the same time.");
        }
    }

    public class FreeboxDownloadSettings : IProviderConfig
    {
        private static readonly FreeboxDownloadSettingsValidator Validator = new FreeboxDownloadSettingsValidator();

        public FreeboxDownloadSettings()
        {
            Host = "mafreebox.freebox.fr";
            Port = 443;
            UseSsl = true;
            ApiUrl = "/api/v1/";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox, HelpText = "DownloadClientFreeboxSettingsHostHelpText")]
        [FieldToken(TokenField.HelpText, "Host", "url", "mafreebox.freebox.fr")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox, HelpText = "DownloadClientFreeboxSettingsPortHelpText")]
        [FieldToken(TokenField.HelpText, "Port", "port", 443)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "Freebox API")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "DownloadClientFreeboxSettingsApiUrl", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientFreeboxSettingsApiUrlHelpText")]
        [FieldToken(TokenField.HelpText, "DownloadClientFreeboxSettingsApiUrl", "url", "http://[host]:[port]/[api_base_url]/[api_version]/")]
        [FieldToken(TokenField.HelpText, "DownloadClientFreeboxSettingsApiUrl", "defaultApiUrl", "/api/v1/")]
        public string ApiUrl { get; set; }

        [FieldDefinition(4, Label = "DownloadClientFreeboxSettingsAppId", Type = FieldType.Textbox, HelpText = "DownloadClientFreeboxSettingsAppIdHelpText")]
        public string AppId { get; set; }

        [FieldDefinition(5, Label = "DownloadClientFreeboxSettingsAppToken", Type = FieldType.Password, Privacy = PrivacyLevel.Password, HelpText = "DownloadClientFreeboxSettingsAppTokenHelpText")]
        public string AppToken { get; set; }

        [FieldDefinition(6, Label = "Destination", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsDestinationHelpText")]
        public string DestinationDirectory { get; set; }

        [FieldDefinition(7, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategorySubFolderHelpText")]
        public string Category { get; set; }

        [FieldDefinition(8, Label = "DownloadClientSettingsRecentPriority", Type = FieldType.Select, SelectOptions = typeof(FreeboxDownloadPriority), HelpText = "DownloadClientSettingsRecentPriorityEpisodeHelpText")]
        public int RecentPriority { get; set; }

        [FieldDefinition(9, Label = "DownloadClientSettingsOlderPriority", Type = FieldType.Select, SelectOptions = typeof(FreeboxDownloadPriority), HelpText = "DownloadClientSettingsOlderPriorityEpisodeHelpText")]
        public int OlderPriority { get; set; }

        [FieldDefinition(10, Label = "DownloadClientSettingsAddPaused", Type = FieldType.Checkbox)]
        public bool AddPaused { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
