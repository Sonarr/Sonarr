using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationSettingsValidator : AbstractValidator<DownloadStationSettings>
    {
        public DownloadStationSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.TvDirectory).Matches(@"^(?!/).+")
                                       .When(c => c.TvDirectory.IsNotNullOrWhiteSpace())
                                       .WithMessage("Cannot start with /");

            RuleFor(c => c.TvCategory).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters a-z and -");

            RuleFor(c => c.TvCategory).Empty()
                                      .When(c => c.TvDirectory.IsNotNullOrWhiteSpace())
                                      .WithMessage("Cannot use Category and Directory");
        }
    }

    public class DownloadStationSettings : IProviderConfig
    {
        private static readonly DownloadStationSettingsValidator Validator = new DownloadStationSettingsValidator();

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Sonarr avoids conflicts with unrelated downloads, but it's optional. Creates a [category] subdirectory in the output directory.")]
        public string TvCategory { get; set; }

        [FieldDefinition(5, Label = "Directory", Type = FieldType.Textbox, HelpText = "Optional shared folder to put downloads into, leave blank to use the default Download Station location")]
        public string TvDirectory { get; set; }

        [FieldDefinition(6, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        public DownloadStationSettings()
        {
            this.Host = "127.0.0.1";
            this.Port = 5000;
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
