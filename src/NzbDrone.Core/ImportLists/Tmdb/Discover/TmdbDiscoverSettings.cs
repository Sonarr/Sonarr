using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverSettingsValidator : TmdbSettingsBaseValidator<TmdbDiscoverSettings>
{
    private static readonly Regex AndOrDelimitedIdsRegex = new(@"^\d+(?:[,|]\d+)*$", RegexOptions.Compiled);

    public TmdbDiscoverSettingsValidator()
    {
        RuleFor(c => c.MinimumVoteAverage).Custom(ValidateVoteAverage);

        RuleFor(c => c.WithKeywords).Matches(AndOrDelimitedIdsRegex)
            .When(c => c.WithKeywords.IsNotNullOrWhiteSpace());

        RuleFor(c => c.WithCompanies).Matches(AndOrDelimitedIdsRegex)
            .When(c => c.WithCompanies.IsNotNullOrWhiteSpace());
    }

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
        OriginalLanguage = (int)TmdbLanguage.Any;
        SortType = (int)TmdbDiscoverSortType.Popularity;
        SortOrderType = (int)TmdbDiscoverSortOrderType.Descending;
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsOriginalLanguage", HelpText  = "ImportListsTmdbSettingsOriginalLanguageHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbLanguage))]
    public int OriginalLanguage { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsSortType", HelpText = "ImportListsTmdbSettingsSortTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSortType))]
    public int SortType { get; set; }

    [FieldDefinition(3, Label = "ImportListsTmdbSettingsSortOrderType", HelpText = "ImportListsTmdbSettingsSortOrderTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSortOrderType))]
    public int SortOrderType { get; set; }

    [FieldDefinition(4, Label = "ImportListsTmdbSettingsMinimumVoteAverage", HelpText = "ImportListsTmdbSettingsMinimumVoteAverageHelpText", Type = FieldType.Textbox)]
    public string MinimumVoteAverage { get; set; }

    [FieldDefinition(5, Label = "ImportListsTmdbSettingsMinimumVoteCount", HelpText = "ImportListsTmdbSettingsMinimumVoteCountHelpText", Type = FieldType.Textbox)]
    public string MinimumVoteCount { get; set; }

    [FieldDefinition(6, Label = "ImportListsTmdbSettingsWithKeywords", HelpText = "ImportListsTmdbSettingsWithAdvancedQueryHelpText", Type = FieldType.Textbox)]
    public string WithKeywords { get; set; }

    [FieldDefinition(7, Label = "ImportListsTmdbSettingsWithCompanies", HelpText = "ImportListsTmdbSettingsWithAdvancedQueryHelpText", Type = FieldType.Textbox)]
    public string WithCompanies { get; set; }

    [FieldDefinition(8, Label = "ImportListsTmdbSettingsIncludeNullFirstAirDates", HelpText = "ImportListsTmdbSettingsIncludeNullFirstAirDatesHelpText", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeNullFirstAirDates { get; set; }
}
