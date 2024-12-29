using FluentValidation;
using Workarr.Annotations;
using Workarr.Validation;

namespace Workarr.ImportLists.Plex
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

    public class PlexListSettings : ImportListSettingsBase<PlexListSettings>
    {
        private static readonly PlexListSettingsValidator Validator = new ();

        public PlexListSettings()
        {
            SignIn = "startOAuth";
        }

        public virtual string Scope => "";

        public override string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(99, Label = "ImportListsPlexSettingsAuthenticateWithPlex", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
