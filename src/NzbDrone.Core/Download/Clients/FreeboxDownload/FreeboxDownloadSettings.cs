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

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox, HelpText = "Hostname or host IP address of the Freebox, defaults to 'mafreebox.freebox.fr' (will only work if on same network)")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox, HelpText = "Port used to access Freebox interface, defaults to '443'")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use secured connection when connecting to Freebox API")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "API URL", Type = FieldType.Textbox, Advanced = true, HelpText = "Define Freebox API base URL with API version, eg http://[host]:[port]/[api_base_url]/[api_version]/, defaults to '/api/v1/'")]
        public string ApiUrl { get; set; }

        [FieldDefinition(4, Label = "App ID", Type = FieldType.Textbox, HelpText = "App ID given when creating access to Freebox API (ie 'app_id')")]
        public string AppId { get; set; }

        [FieldDefinition(5, Label = "App Token", Type = FieldType.Password, Privacy = PrivacyLevel.Password, HelpText = "App token retrieved when creating access to Freebox API (ie 'app_token')")]
        public string AppToken { get; set; }

        [FieldDefinition(6, Label = "Destination Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default Freebox download location")]
        public string DestinationDirectory { get; set; }

        [FieldDefinition(7, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Sonarr avoids conflicts with unrelated non-Sonarr downloads (will create a [category] subdirectory in the output directory)")]
        public string Category { get; set; }

        [FieldDefinition(8, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(FreeboxDownloadPriority), HelpText = "Priority to use when grabbing episodes that aired within the last 14 days")]
        public int RecentPriority { get; set; }

        [FieldDefinition(9, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(FreeboxDownloadPriority), HelpText = "Priority to use when grabbing episodes that aired over 14 days ago")]
        public int OlderPriority { get; set; }

        [FieldDefinition(10, Label = "Add Paused", Type = FieldType.Checkbox)]
        public bool AddPaused { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
