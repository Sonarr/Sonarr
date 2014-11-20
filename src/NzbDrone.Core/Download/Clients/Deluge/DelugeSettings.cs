using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeSettingsValidator : AbstractValidator<DelugeSettings>
    {
        public DelugeSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).GreaterThan(0);

            RuleFor(c => c.TvCategory).Matches("^[-a-z]*$").WithMessage("Allowed characters a-z and -");
        }
    }

    public class DelugeSettings : IProviderConfig
    {
        private static readonly DelugeSettingsValidator validator = new DelugeSettingsValidator();

        public DelugeSettings()
        {
            Host = "localhost";
            Port = 8112;
            Password = "deluge";
            TvCategory = "tv-drone";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [FieldDefinition(3, Label = "Category", Type = FieldType.Textbox)]
        public String TvCategory { get; set; }

        [FieldDefinition(4, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(DelugePriority), HelpText = "Priority to use when grabbing episodes that aired within the last 14 days")]
        public Int32 RecentTvPriority { get; set; }

        [FieldDefinition(5, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(DelugePriority), HelpText = "Priority to use when grabbing episodes that aired over 14 days ago")]
        public Int32 OlderTvPriority { get; set; }

        [FieldDefinition(6, Label = "Use SSL", Type = FieldType.Checkbox)]
        public Boolean UseSsl { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}
