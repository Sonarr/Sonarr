using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetSettingsValidator : AbstractValidator<NzbgetSettings>
    {
        public NzbgetSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Username).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.Password));
            RuleFor(c => c.Password).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.Username));

            RuleFor(c => c.TvCategory).NotEmpty().WithMessage("A category is recommended").AsWarning();
        }
    }

    public class NzbgetSettings : IProviderConfig
    {
        private static readonly NzbgetSettingsValidator Validator = new NzbgetSettingsValidator();

        public NzbgetSettings()
        {
            Host = "localhost";
            Port = 6789;
            TvCategory = "tv";
            RecentTvPriority = (int)NzbgetPriority.Normal;
            OlderTvPriority = (int)NzbgetPriority.Normal;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "NZBGet")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "NZBGet")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/jsonrpc")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string TvCategory { get; set; }

        [FieldDefinition(7, Label = "DownloadClientSettingsRecentPriority", Type = FieldType.Select, SelectOptions = typeof(NzbgetPriority), HelpText = "DownloadClientSettingsRecentPriorityEpisodeHelpText")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(8, Label = "DownloadClientSettingsOlderPriority", Type = FieldType.Select, SelectOptions = typeof(NzbgetPriority), HelpText = "DownloadClientSettingsOlderPriorityEpisodeHelpText")]
        public int OlderTvPriority { get; set; }

        [FieldDefinition(9, Label = "DownloadClientSettingsAddPaused", Type = FieldType.Checkbox, HelpText = "DownloadClientNzbgetSettingsAddPausedHelpText")]
        public bool AddPaused { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
