using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverSettingsValidator : TmdbSettingsBaseValidator<TmdbDiscoverSettings>
{
    private static readonly Regex AndDelimitedIdsRegex = new(@"^\d+(?:,\d+)*$", RegexOptions.Compiled);
    private static readonly Regex AndOrDelimitedIdsRegex = new(@"^\d+(?:[,|]\d+)*$", RegexOptions.Compiled);

    public TmdbDiscoverSettingsValidator()
    {
        RuleFor(c => c.VoteAverageMinimum).Custom(ValidateVoteAverage);

        RuleFor(c => c.AirDateMinimum).Must(ValidateAirDate)
            .WithMessage("Must be in the format: yyyy-MM-dd");

        RuleFor(c => c.AirDateMaximum).Must(ValidateAirDate)
            .WithMessage("Must be in the format: yyyy-MM-dd");

        RuleFor(c => c.WithKeywords).Matches(AndOrDelimitedIdsRegex)
            .When(c => c.WithKeywords.IsNotNullOrWhiteSpace());

        RuleFor(c => c.WithCompanies).Matches(AndOrDelimitedIdsRegex)
            .When(c => c.WithCompanies.IsNotNullOrWhiteSpace());

        RuleFor(c => c.WithNetworks).Matches(AndDelimitedIdsRegex)
            .When(c => c.WithNetworks.IsNotNullOrWhiteSpace());
    }

    private static bool ValidateAirDate(string airDate)
    {
        if (airDate.IsNullOrWhiteSpace())
        {
            return true;
        }

        return DateTime.TryParseExact(airDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
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
        WithGenreTypes = [];
        OriginalLanguage = (int)TmdbLanguage.Any;
        SortByType = (int)TmdbDiscoverSortByType.PopularityDesc;
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsOriginalLanguage", HelpText  = "ImportListsTmdbSettingsOriginalLanguageHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbLanguage))]
    public int OriginalLanguage { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsSortByType", HelpText = "ImportListsTmdbSettingsSortByTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSortByType))]
    public int SortByType { get; set; }

    [FieldDefinition(3, Label = "ImportListsTmdbSettingsVoteAverageMinimum", HelpText = "ImportListsTmdbSettingsVoteAverageMinimumHelpText", Type = FieldType.Textbox)]
    public string VoteAverageMinimum { get; set; }

    [FieldDefinition(4, Label = "ImportListsTmdbSettingsVoteCountMinimum", HelpText = "ImportListsTmdbSettingsVoteCountMinimumHelpText", Type = FieldType.Textbox, Advanced = true)]
    public string VoteCountMinimum { get; set; }

    [FieldDefinition(5, Label = "ImportListsTmdbSettingsAirDateMinimum", HelpText = "ImportListsTmdbSettingsAirDateMinimumHelpText", Type = FieldType.Textbox, Advanced = true)]
    public string AirDateMinimum { get; set; }

    [FieldDefinition(6, Label = "ImportListsTmdbSettingsAirDateMaximum", HelpText = "ImportListsTmdbSettingsAirDateMaximumHelpText", Type = FieldType.Textbox, Advanced = true)]
    public string AirDateMaximum { get; set; }

    [FieldDefinition(7, Label = "ImportListsTmdbSettingsWithGenreTypes", HelpText = "ImportListsTmdbSettingsWithGenreTypesHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverGenreType))]
    public IEnumerable<int> WithGenreTypes { get; set; }

    [FieldDefinition(8, Label = "ImportListsTmdbSettingsWithKeywords", HelpText = "ImportListsTmdbSettingsAndOrDelimitedIdsHelpText", Type = FieldType.Textbox)]
    public string WithKeywords { get; set; }

    [FieldDefinition(9, Label = "ImportListsTmdbSettingsWithCompanies", HelpText = "ImportListsTmdbSettingsAndOrDelimitedIdsHelpText", Type = FieldType.Textbox)]
    public string WithCompanies { get; set; }

    [FieldDefinition(10, Label = "ImportListsTmdbSettingsWithNetworks", HelpText = "ImportListsTmdbSettingsAndDelimitedIdsHelpText", Type = FieldType.Textbox)]
    public string WithNetworks { get; set; }

    [FieldDefinition(11, Label = "ImportListsTmdbSettingsIncludeNullFirstAirDates", HelpText = "ImportListsTmdbSettingsIncludeNullFirstAirDatesHelpText", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeNullFirstAirDates { get; set; }
}
