using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdSettingsValidator : AbstractValidator<SabnzbdSettings>
    {
        public SabnzbdSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).GreaterThan(0);

            RuleFor(c => c.ApiKey).NotEmpty()
                                  .WithMessage("API Key is required when username/password are not configured")
                                  .When(c => String.IsNullOrWhiteSpace(c.Username));

            RuleFor(c => c.Username).NotEmpty()
                                    .WithMessage("Username is required when API key is not configured")
                                    .When(c => String.IsNullOrWhiteSpace(c.ApiKey));


            RuleFor(c => c.Password).NotEmpty()
                                    .WithMessage("Password is required when API key is not configured")
                                    .When(c => String.IsNullOrWhiteSpace(c.ApiKey));
        }
    }

    public class SabnzbdSettings : IProviderConfig
    {
        private static readonly SabnzbdSettingsValidator Validator = new SabnzbdSettingsValidator();

        public SabnzbdSettings()
        {
            Host = "localhost";
            Port = 8080;
            TvCategory = "tv";
            RecentTvPriority = (int)SabnzbdPriority.Default;
            OlderTvPriority = (int)SabnzbdPriority.Default;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public Int32 Port { get; set; }

        [FieldDefinition(2, Label = "API Key", Type = FieldType.Textbox)]
        public String ApiKey { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox)]
        public String Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public String Password { get; set; }

        [FieldDefinition(5, Label = "Category", Type = FieldType.Textbox)]
        public String TvCategory { get; set; }

        [FieldDefinition(6, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority))]
        public Int32 RecentTvPriority { get; set; }

        [FieldDefinition(7, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority))]
        public Int32 OlderTvPriority { get; set; }

        [FieldDefinition(8, Label = "Use SSL", Type = FieldType.Checkbox)]
        public Boolean UseSsl { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
