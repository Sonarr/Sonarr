using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexListSettingsValidator : AbstractValidator<PlexListSettings>
    {
        public PlexListSettingsValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty()
                           .OverridePropertyName("SignIn")
                           .WithMessage("Must authenticate with Plex");
        }
    }

    public class PlexListSettings : IImportListSettings
    {
        protected virtual PlexListSettingsValidator Validator => new PlexListSettingsValidator();

        public PlexListSettings()
        {
            SignIn = "startOAuth";
        }

        public virtual string Scope => "";

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Plex.tv", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
