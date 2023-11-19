using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexSettingsValidator : AbstractValidator<NzbVortexSettings>
    {
        public NzbVortexSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.ApiKey).NotEmpty()
                                  .WithMessage("API Key is required");

            RuleFor(c => c.TvCategory).NotEmpty()
                                      .WithMessage("A category is recommended")
                                      .AsWarning();
        }
    }

    public class NzbVortexSettings : IProviderConfig
    {
        private static readonly NzbVortexSettingsValidator Validator = new NzbVortexSettingsValidator();

        public NzbVortexSettings()
        {
            Host = "localhost";
            Port = 4321;
            TvCategory = "TV Shows";
            RecentTvPriority = (int)NzbVortexPriority.Normal;
            OlderTvPriority = (int)NzbVortexPriority.Normal;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "NZBVortex")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/api")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "ApiKey", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Group", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string TvCategory { get; set; }

        [FieldDefinition(5, Label = "DownloadClientSettingsRecentPriority", Type = FieldType.Select, SelectOptions = typeof(NzbVortexPriority), HelpText = "DownloadClientSettingsRecentPriorityEpisodeHelpText")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(6, Label = "DownloadClientSettingsOlderPriority", Type = FieldType.Select, SelectOptions = typeof(NzbVortexPriority), HelpText = "DownloadClientSettingsOlderPriorityEpisodeHelpText")]
        public int OlderTvPriority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
