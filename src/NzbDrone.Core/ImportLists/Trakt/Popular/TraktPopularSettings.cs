using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularSettingsValidator : TraktSettingsBaseValidator<TraktPopularSettings>
    {
        public TraktPopularSettingsValidator()
        : base()
        {
            RuleFor(c => c.TraktListType).NotNull();

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
        }
    }

    public class TraktPopularSettings : TraktSettingsBase<TraktPopularSettings>
    {
        protected override AbstractValidator<TraktPopularSettings> Validator => new TraktPopularSettingsValidator();

        public TraktPopularSettings()
        {
            TraktListType = (int)TraktPopularListType.Popular;
        }

        [FieldDefinition(1, Label = "ImportListsTraktSettingsListType", Type = FieldType.Select, SelectOptions = typeof(TraktPopularListType), HelpText = "ImportListsTraktSettingsListTypeHelpText")]
        public int TraktListType { get; set; }

        [FieldDefinition(2, Label = "ImportListsTraktSettingsRating", HelpText = "ImportListsTraktSettingsRatingHelpText")]
        public string Rating { get; set; }

        [FieldDefinition(4, Label = "ImportListsTraktSettingsGenres", HelpText = "ImportListsTraktSettingsGenresHelpText")]
        public string Genres { get; set; }

        [FieldDefinition(5, Label = "ImportListsTraktSettingsYears", HelpText = "ImportListsTraktSettingsYearsHelpText")]
        public string Years { get; set; }
    }
}
