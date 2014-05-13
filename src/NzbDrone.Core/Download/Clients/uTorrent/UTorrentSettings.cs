using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrentSettingsValidator : AbstractValidator<UTorrentSettings>
    {
        public UTorrentSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(0, 65535);
            RuleFor(c => c.TvCategory).NotEmpty();
        }
    }

    public class UTorrentSettings : IProviderConfig
    {
        private static readonly UTorrentSettingsValidator validator = new UTorrentSettingsValidator();

        public UTorrentSettings()
        {
            Host = "localhost";
            Port = 9091;
            TvCategory = "tv-drone";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox)]
        public String Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [FieldDefinition(4, Label = "Category", Type = FieldType.Textbox)]
        public String TvCategory { get; set; }

        [FieldDefinition(5, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(UTorrentPriority), HelpText = "Priority to use when grabbing episodes that aired within the last 14 days")]
        public Int32 RecentTvPriority { get; set; }

        [FieldDefinition(6, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(UTorrentPriority), HelpText = "Priority to use when grabbing episodes that aired over 14 days ago")]
        public Int32 OlderTvPriority { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}
