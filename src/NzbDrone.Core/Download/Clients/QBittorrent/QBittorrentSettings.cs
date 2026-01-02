using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public class QBittorrentSettingsValidator : AbstractValidator<QBittorrentSettings>
    {
        public QBittorrentSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Username).Empty()
                .WithMessage("Username must be empty when using API Key.")
                .When(c => c.ApiKey.IsNotNullOrWhiteSpace());
            RuleFor(c => c.Password).Empty()
                .WithMessage("Password must be empty when using API Key.")
                .When(c => c.ApiKey.IsNotNullOrWhiteSpace());

            RuleFor(c => c.TvCategory).Matches(@"^([^\\\/](\/?[^\\\/])*)?$").WithMessage(@"Can not contain '\', '//', or start/end with '/'");
            RuleFor(c => c.TvImportedCategory).Matches(@"^([^\\\/](\/?[^\\\/])*)?$").WithMessage(@"Can not contain '\', '//', or start/end with '/'");
        }
    }

    public class QBittorrentSettings : DownloadClientSettingsBase<QBittorrentSettings>
    {
        private static readonly QBittorrentSettingsValidator Validator = new();

        public QBittorrentSettings()
        {
            Host = "localhost";
            Port = 8080;
            TvCategory = "tv-sonarr";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientQbittorrentSettingsUseSslHelpText")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "qBittorrent")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/api")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "ApiKey", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(5, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(6, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(7, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string TvCategory { get; set; }

        [FieldDefinition(8, Label = "PostImportCategory", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsPostImportCategoryHelpText")]
        public string TvImportedCategory { get; set; }

        [FieldDefinition(9, Label = "DownloadClientSettingsRecentPriority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "DownloadClientSettingsRecentPriorityEpisodeHelpText")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(10, Label = "DownloadClientSettingsOlderPriority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "DownloadClientSettingsOlderPriorityEpisodeHelpText")]
        public int OlderTvPriority { get; set; }

        [FieldDefinition(11, Label = "DownloadClientSettingsInitialState", Type = FieldType.Select, SelectOptions = typeof(QBittorrentState), HelpText = "DownloadClientQbittorrentSettingsInitialStateHelpText")]
        public int InitialState { get; set; }

        [FieldDefinition(12, Label = "DownloadClientQbittorrentSettingsSequentialOrder", Type = FieldType.Checkbox, HelpText = "DownloadClientQbittorrentSettingsSequentialOrderHelpText")]
        public bool SequentialOrder { get; set; }

        [FieldDefinition(13, Label = "DownloadClientQbittorrentSettingsFirstAndLastFirst", Type = FieldType.Checkbox, HelpText = "DownloadClientQbittorrentSettingsFirstAndLastFirstHelpText")]
        public bool FirstAndLast { get; set; }

        [FieldDefinition(14, Label = "DownloadClientQbittorrentSettingsContentLayout", Type = FieldType.Select, SelectOptions = typeof(QBittorrentContentLayout), HelpText = "DownloadClientQbittorrentSettingsContentLayoutHelpText")]
        public int ContentLayout { get; set; }

        [FieldDefinition(15, Label = "DownloadClientQbittorrentSettingsAddSeriesTags", Type = FieldType.Checkbox, HelpText = "DownloadClientQbittorrentSettingsAddSeriesTagsHelpText")]
        public bool AddSeriesTags { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
