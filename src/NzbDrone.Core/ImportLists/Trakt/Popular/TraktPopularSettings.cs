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

            // Any valid certification
            RuleFor(c => c.Certification)
                .Matches(@"^\bNR\b|\bG\b|\bPG\b|\bPG\-13\b|\bR\b|\bNC\-17\b$", RegexOptions.IgnoreCase)
                .When(c => c.Certification.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid cerification");

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

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktPopularListType), HelpText = "Type of list you're seeking to import from")]
        public int TraktListType { get; set; }

        [FieldDefinition(2, Label = "Rating", HelpText = "Filter movies by rating range (0-100)")]
        public string Rating { get; set; }

        [FieldDefinition(3, Label = "Certification", HelpText = "Filter movies by a certification (NR,G,PG,PG-13,R,NC-17), (Comma Separated)")]
        public string Certification { get; set; }

        [FieldDefinition(4, Label = "Genres", HelpText = "Filter movies by Trakt Genre Slug (Comma Separated) Only for Popular Lists")]
        public string Genres { get; set; }

        [FieldDefinition(5, Label = "Years", HelpText = "Filter movies by year or year range")]
        public string Years { get; set; }
    }
}
