using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Trakt
{
    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.RefreshToken).NotEmpty();
            RuleFor(c => c.Expires).NotEmpty();
        }
    }

    public class TraktSettings : IProviderConfig
    {
        private static readonly TraktSettingsValidator Validator = new TraktSettingsValidator();

        public TraktSettings()
        {
            SignIn = "startOAuth";
        }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(1, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(2, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(3, Label = "Auth User", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(4, Label = "Authenticate with Trakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
