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

    public class AniListSettingsBase<TSettings> : IImportListSettings
        where TSettings : AniListSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new AniListSettingsBaseValidator<TSettings>();

        public AniListSettingsBase()
        {
            BaseUrl = "https://graphql.anilist.co";
            SignIn = "startOAuth";
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(99, Label = "Authenticate with AniList", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
