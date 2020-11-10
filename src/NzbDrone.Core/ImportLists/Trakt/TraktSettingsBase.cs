using System;
using System.Text.RegularExpressions;
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

            // Loose validation @TODO
            RuleFor(c => c.Rating)
                .Matches(@"^\d+\-\d+$", RegexOptions.IgnoreCase)
                .When(c => c.Rating.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid rating");

            // Loose validation @TODO
            RuleFor(c => c.Years)
                .Matches(@"^\d+(\-\d+)?$", RegexOptions.IgnoreCase)
                .When(c => c.Years.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid year or range of years");

            // Limit not smaller than 1 and not larger than 100
            RuleFor(c => c.Limit)
                .GreaterThan(0)
                .WithMessage("Must be integer greater than 0");
        }
    }

    public class TraktSettingsBase<TSettings> : IImportListSettings
        where TSettings : TraktSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new TraktSettingsBaseValidator<TSettings>();

        public TraktSettingsBase()
        {
            BaseUrl = "https://api.trakt.tv";
            SignIn = "startOAuth";
            Rating = "0-100";
            Genres = "";
            Years = "";
            Limit = 100;
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(0, Label = "Auth User", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(1, Label = "Rating", HelpText = "Filter series by rating range (0-100)")]
        public string Rating { get; set; }

        [FieldDefinition(3, Label = "Genres", HelpText = "Filter series by Trakt Genre Slug (Comma Separated)")]
        public string Genres { get; set; }

        [FieldDefinition(4, Label = "Years", HelpText = "Filter series by year or year range")]
        public string Years { get; set; }

        [FieldDefinition(5, Label = "Limit", HelpText = "Limit the number of series to get")]
        public int Limit { get; set; }

        [FieldDefinition(6, Label = "Additional Parameters", HelpText = "Additional Trakt API parameters", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Trakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
