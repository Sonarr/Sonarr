using System;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.AniList
{
    public class AniListSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : AniListSettingsBase<TSettings>
    {
        public AniListSettingsBaseValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.AccessToken).NotEmpty()
                                       .OverridePropertyName("SignIn")
                                       .WithMessage("Must authenticate with AniList");

            RuleFor(c => c.RefreshToken).NotEmpty()
                                        .OverridePropertyName("SignIn")
                                        .WithMessage("Must authenticate with AniList")
                                        .When(c => c.AccessToken.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Expires).NotEmpty()
                                   .OverridePropertyName("SignIn")
                                   .WithMessage("Must authenticate with AniList")
                                   .When(c => c.AccessToken.IsNotNullOrWhiteSpace() && c.RefreshToken.IsNotNullOrWhiteSpace());
        }
    }

    public class AniListSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : AniListSettingsBase<TSettings>
    {
        private static readonly AniListSettingsBaseValidator<TSettings> Validator = new ();

        public AniListSettingsBase()
        {
            SignIn = "startOAuth";
        }

        public override string BaseUrl { get; set; } = "https://graphql.anilist.co";

        [FieldDefinition(0, Label = "ImportListsSettingsAccessToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsRefreshToken", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "ImportListsSettingsExpires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(99, Label = "ImportListsAniListSettingsAuthenticateWithAniList", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
