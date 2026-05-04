using System;
using System.Globalization;
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

            RuleFor(c => c.Rating)
                .Must(BeValidRatingRange)
                .When(c => c.Rating.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid rating");

            RuleFor(c => c.TraktAdditionalParameters)
                .Must(additionalParameters => !TraktQueryHelper.ContainsReservedFilterParameters(additionalParameters))
                .When(c => c.TraktAdditionalParameters.IsNotNullOrWhiteSpace())
                .WithMessage("Additional parameters cannot include genres, ratings, years, or limit");
        }

        protected static bool BeValidYearRange(string years)
        {
            var parts = years.Split('-', StringSplitOptions.None);

            if (parts.Length == 1)
            {
                return TryParseYear(parts[0], out _);
            }

            if (parts.Length != 2)
            {
                return false;
            }

            return TryParseYear(parts[0], out var startYear) &&
                   TryParseYear(parts[1], out var endYear) &&
                   startYear <= endYear;
        }

        private static bool BeValidRatingRange(string rating)
        {
            var parts = rating.Split('-', StringSplitOptions.None);

            if (parts.Length != 2)
            {
                return false;
            }

            return TryParseRating(parts[0], out var minimumRating) &&
                   TryParseRating(parts[1], out var maximumRating) &&
                   minimumRating <= maximumRating;
        }

        private static bool TryParseYear(string token, out int year)
        {
            year = default;

            return token.Length == 4 &&
                   int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out year) &&
                   year >= 1000;
        }

        private static bool TryParseRating(string token, out int rating)
        {
            if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out rating))
            {
                return false;
            }

            if (rating is < 0 or > 100)
            {
                return false;
            }

            return token == rating.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class TraktSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : TraktSettingsBase<TSettings>
    {
        private static readonly TraktSettingsBaseValidator<TSettings> Validator = new();

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

        [FieldDefinition(95, Label = "ImportListsTraktSettingsRating", HelpText = "ImportListsTraktSettingsRatingSeriesHelpText")]
        public string Rating { get; set; }

        [FieldDefinition(96, Label = "ImportListsTraktSettingsGenres", HelpText = "ImportListsTraktSettingsGenresSeriesHelpText")]
        public string Genres { get; set; }

        [FieldDefinition(97, Label = "ImportListsTraktSettingsAdditionalParameters", HelpText = "ImportListsTraktSettingsAdditionalParametersHelpText", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        [FieldDefinition(98, Label = "ImportListsTraktSettingsLimit", HelpText = "ImportListsTraktSettingsLimitSeriesHelpText")]
        public int Limit { get; set; }

        [FieldDefinition(99, Label = "ImportListsTraktSettingsAuthenticateWithTrakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
