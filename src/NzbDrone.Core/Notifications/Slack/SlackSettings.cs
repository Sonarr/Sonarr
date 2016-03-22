using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Notifications.Slack
{
    public class SlackSettingsValidator : AbstractValidator<SlackSettings>
    {
        public SlackSettingsValidator()
        {
            RuleFor(c => c.WebHookUrl).IsValidUrl();
            RuleFor(c => c.BotName).NotEmpty();
            RuleFor(c => c.Icon).NotEmpty();
            RuleFor(c => c.OnGrabPayload).NotEmpty();
            RuleFor(c => c.OnDownloadPayload).NotEmpty();
            RuleFor(c => c.OnRenamePayload).NotEmpty();
        }
    }

    public class SlackSettings : IProviderConfig
    {
        private static readonly SlackSettingsValidator Validator = new SlackSettingsValidator();

        [FieldDefinition(0, Label = "WebHookUrl", HelpText = "slack channel webhook url.", Type = FieldType.Url)]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "BotName", HelpText = "Name to be used for the notification.",Type = FieldType.Textbox)]
        public string BotName { get; set; }

        [FieldDefinition(2, Label = "Icon", HelpText = "Icon to use.", Type = FieldType.Textbox)]
        public string Icon { get; set; }

        [FieldDefinition(3, Label = "OnGrabPayload", HelpText = "Text to be posted on slack on grab.", Type = FieldType.Textbox)]
        public string OnGrabPayload { get; set; }

        [FieldDefinition(4, Label = "OnDownloadPayload", HelpText = "Text to be posted on slack on download.", Type = FieldType.Textbox)]
        public string OnDownloadPayload { get; set; }

        [FieldDefinition(5, Label = "OnRenamePayload", HelpText = "Text to be posted on slack on rename.", Type = FieldType.Textbox)]
        public string OnRenamePayload { get; set; }



        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
