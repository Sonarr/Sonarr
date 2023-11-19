using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public class RTorrentSettingsValidator : AbstractValidator<RTorrentSettings>
    {
        public RTorrentSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.TvCategory).NotEmpty()
                                      .WithMessage("A category is recommended")
                                      .AsWarning();
        }
    }

    public class RTorrentSettings : IProviderConfig
    {
        private static readonly RTorrentSettingsValidator Validator = new RTorrentSettingsValidator();

        public RTorrentSettings()
        {
            Host = "localhost";
            Port = 8080;
            UrlBase = "RPC2";
            TvCategory = "tv-sonarr";
            OlderTvPriority = (int)RTorrentPriority.Normal;
            RecentTvPriority = (int)RTorrentPriority.Normal;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "clientName", "rTorrent")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "DownloadClientRTorrentSettingsUrlPath", Type = FieldType.Textbox, HelpText = "DownloadClientRTorrentSettingsUrlPathHelpText")]
        [FieldToken(TokenField.HelpText, "DownloadClientRTorrentSettingsUrlPath", "url", "http(s)://[host]:[port]/[urlPath]")]
        [FieldToken(TokenField.HelpText, "DownloadClientRTorrentSettingsUrlPath", "url2", "/plugins/rpc/rpc.php")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "DownloadClientSettingsCategoryHelpText")]
        public string TvCategory { get; set; }

        [FieldDefinition(7, Label = "PostImportCategory", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsPostImportCategoryHelpText")]
        public string TvImportedCategory { get; set; }

        [FieldDefinition(8, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientRTorrentSettingsDirectoryHelpText")]
        public string TvDirectory { get; set; }

        [FieldDefinition(9, Label = "DownloadClientSettingsRecentPriority", Type = FieldType.Select, SelectOptions = typeof(RTorrentPriority), HelpText = "DownloadClientSettingsRecentPriorityEpisodeHelpText")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(10, Label = "DownloadClientSettingsOlderPriority", Type = FieldType.Select, SelectOptions = typeof(RTorrentPriority), HelpText = "DownloadClientSettingsOlderPriorityEpisodeHelpText")]
        public int OlderTvPriority { get; set; }

        [FieldDefinition(11, Label = "DownloadClientRTorrentSettingsAddStopped", Type = FieldType.Checkbox, HelpText = "DownloadClientRTorrentSettingsAddStoppedHelpText")]
        public bool AddStopped { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
