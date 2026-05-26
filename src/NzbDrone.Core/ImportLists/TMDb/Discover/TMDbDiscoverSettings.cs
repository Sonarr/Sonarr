using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Discover;

public sealed partial class TMDbDiscoverSettingsValidator : TMDbSettingsBaseValidator<TMDbDiscoverSettings>
{
    public TMDbDiscoverSettingsValidator()
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

public sealed class TMDbDiscoverSettings : TMDbSettingsBase<TMDbDiscoverSettings>
{
    private static readonly TMDbDiscoverSettingsValidator Validator = new();

    public TMDbDiscoverSettings()
        : base(Validator)
    {
        Sort = (int)TMDbDiscoverSort.Popularity;
        SortOrder = (int)TMDbDiscoverSortOrder.Descending;
    }

    [FieldDefinition(1, Label = "Include Adult", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeAdult { get; set; }

    [FieldDefinition(2, Label = "Include Without First Air Dates", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeNullFirstAirDates { get; set; }

    [FieldDefinition(3, Label = "Original Language", Type = FieldType.Select, SelectOptions = typeof(TMDbLanguageOptionsConverter))]
    public int WithOriginalLanguageCode { get; set; }

    [FieldDefinition(4, Label = "Sort", Type = FieldType.Select, SelectOptions = typeof(TMDbDiscoverSort))]
    public int Sort { get; set; }

    [FieldDefinition(5, Label = "Sort Order", Type = FieldType.Select, SelectOptions = typeof(TMDbDiscoverSortOrder))]
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
