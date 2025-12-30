using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScriptSettingsValidator : AbstractValidator<CustomScriptSettings>
    {
        public CustomScriptSettingsValidator()
        {
            RuleFor(c => c.Path)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(new SystemFolderValidator())
                .WithMessage("Must not be a descendant of '{systemFolder}'");

            RuleFor(c => c.Arguments)
                .Empty()
                .WithMessage("Arguments are no longer supported for custom scripts");
        }
    }

    public class CustomScriptSettings : NotificationSettingsBase<CustomScriptSettings>
    {
        private static readonly CustomScriptSettingsValidator Validator = new();

        [FieldDefinition(0, Label = "Path", Type = FieldType.FilePath)]
        public string Path { get; set; }

        [FieldDefinition(1, Label = "NotificationsCustomScriptSettingsArguments", HelpText = "NotificationsCustomScriptSettingsArgumentsHelpText", Hidden = HiddenType.HiddenIfNotSet)]
        public string Arguments { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
