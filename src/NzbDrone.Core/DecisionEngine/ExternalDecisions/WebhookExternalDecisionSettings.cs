using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public class WebhookExternalDecisionSettingsValidator : AbstractValidator<WebhookExternalDecisionSettings>
    {
        public WebhookExternalDecisionSettingsValidator()
        {
            RuleFor(c => c.Url).IsValidUrl();
            RuleFor(c => c.Timeout).InclusiveBetween(1, 120);
        }
    }

    public class WebhookExternalDecisionSettings : ExternalDecisionSettingsBase<WebhookExternalDecisionSettings>
    {
        private static readonly WebhookExternalDecisionSettingsValidator Validator = new();

        public WebhookExternalDecisionSettings()
        {
            Timeout = 30;
        }

        [FieldDefinition(0, Label = "ExternalDecisionSettingsUrlLabel", Type = FieldType.Url, HelpText = "ExternalDecisionSettingsUrlHelpText")]
        public string Url { get; set; }

        [FieldDefinition(1, Label = "ExternalDecisionSettingsApiKeyLabel", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "ExternalDecisionSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "ExternalDecisionSettingsTimeoutLabel", Type = FieldType.Number, Unit = "seconds", HelpText = "ExternalDecisionSettingsTimeoutHelpText", Advanced = true)]
        public int Timeout { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
