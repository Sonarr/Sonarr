using FluentValidation;
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

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Sonarr avoids conflicts with unrelated downloads, but it's optional")]
        public string TvCategory { get; set; }

        [FieldDefinition(5, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(NzbgetPriority), HelpText = "Priority to use when grabbing episodes that aired within the last 14 days")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(6, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(NzbgetPriority), HelpText = "Priority to use when grabbing episodes that aired over 14 days ago")]
        public int OlderTvPriority { get; set; }

        [FieldDefinition(7, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(8, Label = "Add Paused", Type = FieldType.Checkbox, HelpText = "This option requires at least NzbGet version 16.0")]
        public bool AddPaused { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
