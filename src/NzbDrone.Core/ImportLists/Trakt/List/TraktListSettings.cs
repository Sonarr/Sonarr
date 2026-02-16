using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListSettingsValidator : TraktSettingsBaseValidator<TraktListSettings>
    {
        public TraktListSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Listname).NotEmpty();

            RuleFor(c => c.Rating)
                .Matches(@"^\d+\-\d+$", RegexOptions.IgnoreCase)
                .When(c => c.Rating.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid rating");

            RuleFor(c => c.Years)
                .Matches(@"^\d+(\-\d+)?$", RegexOptions.IgnoreCase)
                .When(c => c.Years.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid year or range of years");
        }
    }

    public class TraktListSettings : TraktSettingsBase<TraktListSettings>
    {
        private static readonly TraktListSettingsValidator Validator = new();

        [FieldDefinition(1, Label = "Username", HelpText = "ImportListsTraktSettingsUsernameHelpText")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "ImportListsTraktSettingsListName", HelpText = "ImportListsTraktSettingsListNameHelpText")]
        public string Listname { get; set; }

        [FieldDefinition(3, Label = "ImportListsTraktSettingsRating", HelpText = "ImportListsTraktSettingsRatingSeriesHelpText")]
        public string Rating { get; set; }

        [FieldDefinition(4, Label = "ImportListsTraktSettingsGenres", HelpText = "ImportListsTraktSettingsGenresSeriesHelpText")]
        public string Genres { get; set; }

        [FieldDefinition(5, Label = "ImportListsTraktSettingsYears", HelpText = "ImportListsTraktSettingsYearsSeriesHelpText")]
        public string Years { get; set; }

        [FieldDefinition(6, Label = "ImportListsTraktSettingsAdditionalParameters", HelpText = "ImportListsTraktSettingsAdditionalParametersHelpText", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
