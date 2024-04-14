using System;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : TraktSettingsBase<TSettings>
    {
        public TraktSettingsBaseValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with Trakt");

            RuleFor(c => c.RefreshToken).NotEmpty()
                                        .OverridePropertyName("SignIn")
                                        .WithMessage("Must authenticate with Trakt")
                                        .When(c => c.AccessToken.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Expires).NotEmpty()
                                   .OverridePropertyName("SignIn")
                                   .WithMessage("Must authenticate with Trakt")
                                   .When(c => c.AccessToken.IsNotNullOrWhiteSpace() && c.RefreshToken.IsNotNullOrWhiteSpace());

            // Limit not smaller than 1 and not larger than 100
            RuleFor(c => c.Limit)
                .GreaterThan(0)
                .WithMessage("Must be integer greater than 0");
        }
    }

    public class TraktSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : TraktSettingsBase<TSettings>
    {
        private static readonly TraktSettingsBaseValidator<TSettings> Validator = new ();

        public TraktSettingsBase()
        {
            SignIn = "startOAuth";
            Limit = 100;
        }

        public override string BaseUrl { get; set; } = "https://api.trakt.tv";

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsAuthUser", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(5, Label = "ImportListsTraktSettingsLimit", HelpText = "ImportListsTraktSettingsLimitHelpText")]
        public int Limit { get; set; }

        [FieldDefinition(6, Label = "ImportListsTraktSettingsAdditionalParameters", HelpText = "ImportListsTraktSettingsAdditionalParametersHelpText", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        [FieldDefinition(99, Label = "ImportListsTraktSettingsAuthenticateWithTrakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
