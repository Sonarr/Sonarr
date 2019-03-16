using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSettingsValidator : AbstractValidator<OmgwtfnzbsSettings>
    {
        public OmgwtfnzbsSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.Delay).GreaterThanOrEqualTo(0);
        }
    }

    public class OmgwtfnzbsSettings : IIndexerSettings
    {
        private static readonly OmgwtfnzbsSettingsValidator Validator = new OmgwtfnzbsSettingsValidator();

        public OmgwtfnzbsSettings()
        {
            Delay = 30;
            Priority = IndexerDefaults.PRIORITY;
        }

        // Unused since Omg has a hardcoded url.
        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Delay", HelpText = "Time in minutes to delay new nzbs before they appear on the RSS feed", Advanced = true)]
        public int Delay { get; set; }

        [FieldDefinition(3, Label = "Priority", HelpText = "Used when searching for releases, a higher priority indexer will be used if releases found have the same quality", Advanced = true)]
        public int Priority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
