using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public class TriblerSettingsValidator : AbstractValidator<TriblerDownloadSettings>
    {
        public TriblerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.UrlBase).ValidUrlBase();

            RuleFor(c => c.ApiKey).NotEmpty();

            RuleFor(c => c.TvCategory).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters a-z and -");

            RuleFor(c => c.TvCategory).Empty()
                .When(c => c.TvDirectory.IsNotNullOrWhiteSpace())
                .WithMessage("Cannot use Category and Directory");

            RuleFor(c => c.AnonymityLevel).GreaterThanOrEqualTo(0).WithMessage("Should be equal to or more than zero.");

        }
    }

    public class TriblerDownloadSettings : IProviderConfig
    {
        private static readonly TriblerSettingsValidator Validator = new TriblerSettingsValidator();

        public TriblerDownloadSettings()
        {
            Host = "localhost";
            Port = 52194;
            UrlBase = "";

            AnonymityLevel = 1;
            SafeSeeding = true;

        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use secure connection when connecting to Tribler")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the tribler url, eg http://[host]:[port]/[urlBase], defaults to ''")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "ApiKey", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "Api key, found in %APPDATA%\\Roaming\\.Tribler\\7.10\\triblerd.conf, the api key is [api].key, NOT [http_api].key")]
        public string ApiKey { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Sonarr avoids conflicts with unrelated downloads, but it's optional. Creates a [category] subdirectory in the output directory.")]
        public string TvCategory { get; set; }

        [FieldDefinition(7, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default Transmission location")]
        public string TvDirectory { get; set; }

        [FieldDefinition(7, Label = "AnonymityLevel", Type = FieldType.Number, HelpText = "Number of proxies to use when downloading content. To disable set to 0. Proxies reduce download/upload speed. See https://www.tribler.org/anonymity.html")]
        public int AnonymityLevel { get; set; }

        [FieldDefinition(7, Label = "SafeSeeding", Type = FieldType.Checkbox, HelpText = "If seeding only should be done through proxies. The Anonymity level defines the number of proxies used. See https://www.tribler.org/anonymity.html")]
        public bool SafeSeeding { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
