using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public partial class TmdbDiscoverSettingsValidator : TmdbSettingsBaseValidator<TmdbDiscoverSettings>
{
    public TmdbDiscoverSettingsValidator()
    {
        RuleFor(c => c.MinimumVoteAverage).Custom(ValidateVoteAverage);

        RuleFor(c => c.WithKeywords).Matches(WithQueryRegex)
            .When(c => c.WithKeywords.IsNotNullOrWhiteSpace());

        RuleFor(c => c.WithCompanies).Matches(WithQueryRegex)
            .When(c => c.WithCompanies.IsNotNullOrWhiteSpace());
    }

    [GeneratedRegex(@"^\d+(?:[,|]\d+)*$")]
    private static partial Regex WithQueryRegex { get; }

    private static void ValidateVoteAverage(string voteAverage, CustomContext context)
    {
        if (voteAverage.IsNullOrWhiteSpace())
        {
            return;
        }

        if (!float.TryParse(voteAverage, out var voteAverageParsed))
        {
            context.AddFailure("Must be a valid single-precision floating-point number.");
        }
        else if (voteAverageParsed < 0)
        {
            context.AddFailure("Must be greater than or equal to zero. (0.00)");
        }
        else if (voteAverageParsed > 10)
        {
            context.AddFailure("Must be less than or equal to ten. (10.00)");
        }
    }
}

public class TmdbDiscoverSettings : TmdbSettingsBase<TmdbDiscoverSettings>
{
    private static readonly TmdbDiscoverSettingsValidator Validator = new();

    public TmdbDiscoverSettings()
        : base(Validator)
    {
        Sort = (int)TmdbDiscoverSort.Popularity;
        SortOrder = (int)TmdbDiscoverSortOrder.Descending;
    }

    [FieldDefinition(2, Label = "Include Without First Air Dates", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeNullFirstAirDates { get; set; }

    [FieldDefinition(3, Label = "Original Language", Type = FieldType.Select, SelectOptions = typeof(TmdbLanguageOptionsConverter))]
    public int WithOriginalLanguageCode { get; set; }

    [FieldDefinition(4, Label = "Sort", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSort))]
    public int Sort { get; set; }

    [FieldDefinition(5, Label = "Sort Order", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSortOrder))]
    public int SortOrder { get; set; }

    [FieldDefinition(6, Label = "Minimum Vote Average", Type = FieldType.Textbox, HelpText = "Filter series by minimum vote average.")]
    public string MinimumVoteAverage { get; set; }

    [FieldDefinition(7, Label = "Minimum Vote Count", Type = FieldType.Textbox, HelpText = "Filter series by minimum vote count.")]
    public string MinimumVoteCount { get; set; }

    [FieldDefinition(8, Label = "Company Ids", Type = FieldType.Textbox)]
    public string WithCompanies { get; set; }

    [FieldDefinition(9, Label = "Keyword Ids", Type = FieldType.Textbox)]
    public string WithKeywords { get; set; }
}
