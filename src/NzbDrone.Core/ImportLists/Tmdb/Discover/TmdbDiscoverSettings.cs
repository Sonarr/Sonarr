using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public partial class TmdbDiscoverSettingsValidator : TmdbSettingsBaseValidator<TmdbDiscoverSettings>
{
    private const int MaxAirDateOffsetDays = 36500;

    public TmdbDiscoverSettingsValidator()
    {
        RuleFor(c => c.VoteAverageMinimum).Custom(ValidateVoteAverage);

        RuleFor(c => c.VoteCountMinimum).Custom(ValidateVoteCount);

        RuleFor(c => c.AirDateMinimumDays).InclusiveBetween(-MaxAirDateOffsetDays, MaxAirDateOffsetDays)
            .When(c => c.AirDateMinimumDays.HasValue);

        RuleFor(c => c.AirDateMaximumDays).InclusiveBetween(-MaxAirDateOffsetDays, MaxAirDateOffsetDays)
            .When(c => c.AirDateMaximumDays.HasValue);

        RuleFor(c => c.AirDateMaximumDays).GreaterThanOrEqualTo(c => c.AirDateMinimumDays)
            .When(c => c.AirDateMinimumDays.HasValue && c.AirDateMaximumDays.HasValue)
            .WithMessage("Must be greater than or equal to the minimum air date days.");

        RuleFor(c => c.WithKeywords).Matches(AndOrDelimitedIdsRegex())
            .When(c => c.WithKeywords.IsNotNullOrWhiteSpace());

        RuleFor(c => c.WithCompanies).Matches(AndOrDelimitedIdsRegex())
            .When(c => c.WithCompanies.IsNotNullOrWhiteSpace());

        RuleForEach(c => c.WithNetworks).Must(id => int.TryParse(id, out var idInt) && idInt > 0)
            .WithMessage("Must be a valid network id.");
    }

    [GeneratedRegex(@"^\d+(?:[,|]\d+)*$")]
    private static partial Regex AndOrDelimitedIdsRegex();

    private static void ValidateVoteAverage(string voteAverage, CustomContext context)
    {
        if (voteAverage.IsNullOrWhiteSpace())
        {
            return;
        }

        if (!float.TryParse(voteAverage, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var voteAverageParsed) || !float.IsFinite(voteAverageParsed))
        {
            context.AddFailure("Must be a valid single-precision floating-point number.");
            return;
        }

        switch (voteAverageParsed)
        {
            case < 0:
                context.AddFailure("Must be greater than or equal to zero. (0.00)");
                break;
            case > 10:
                context.AddFailure("Must be less than or equal to ten. (10.00)");
                break;
        }
    }

    private static void ValidateVoteCount(string voteCount, CustomContext context)
    {
        if (voteCount.IsNullOrWhiteSpace())
        {
            return;
        }

        if (!int.TryParse(voteCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var voteCountParsed) || voteCountParsed < 0)
        {
            context.AddFailure("Must be a valid whole number greater than or equal to zero.");
        }
    }
}

public class TmdbDiscoverSettings() : TmdbSettingsBase<TmdbDiscoverSettings>(Validator)
{
    private static readonly TmdbDiscoverSettingsValidator Validator = new();

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsOriginalLanguage", HelpText  = "ImportListsTmdbSettingsOriginalLanguageHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbLanguage))]
    public int OriginalLanguage { get; set; } = (int)TmdbLanguage.Any;

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsSortByType", HelpText = "ImportListsTmdbSettingsSortByTypeHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverSortByType))]
    public int SortByType { get; set; } = (int)TmdbDiscoverSortByType.PopularityDesc;

    [FieldDefinition(3, Label = "ImportListsTmdbSettingsVoteAverageMinimum", HelpText = "ImportListsTmdbSettingsVoteAverageMinimumHelpText", Type = FieldType.Textbox)]
    public string VoteAverageMinimum { get; set; }

    [FieldDefinition(4, Label = "ImportListsTmdbSettingsVoteCountMinimum", HelpText = "ImportListsTmdbSettingsVoteCountMinimumHelpText", Type = FieldType.Textbox, Advanced = true)]
    public string VoteCountMinimum { get; set; }

    [FieldDefinition(5, Label = "ImportListsTmdbSettingsAirDateMinimumDays", HelpText = "ImportListsTmdbSettingsAirDateMinimumDaysHelpText", Type = FieldType.Number, Advanced = true)]
    public int? AirDateMinimumDays { get; set; }

    [FieldDefinition(6, Label = "ImportListsTmdbSettingsAirDateMaximumDays", HelpText = "ImportListsTmdbSettingsAirDateMaximumDaysHelpText", Type = FieldType.Number, Advanced = true)]
    public int? AirDateMaximumDays { get; set; }

    [FieldDefinition(7, Label = "ImportListsTmdbSettingsWithGenreTypes", HelpText = "ImportListsTmdbSettingsWithGenreTypesHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDiscoverGenreType))]
    public IEnumerable<int> WithGenreTypes { get; set; } = [];

    [FieldDefinition(8, Label = "ImportListsTmdbSettingsWithKeywords", HelpText = "ImportListsTmdbSettingsAndOrDelimitedIdsHelpText", Type = FieldType.Textbox)]
    public string WithKeywords { get; set; }

    [FieldDefinition(9, Label = "ImportListsTmdbSettingsWithCompanies", HelpText = "ImportListsTmdbSettingsAndOrDelimitedIdsHelpText", Type = FieldType.Textbox)]
    public string WithCompanies { get; set; }

    [FieldDefinition(10, Label = "ImportListsTmdbSettingsWithNetworks", HelpText = "ImportListsTmdbSettingsWithNetworksHelpText", Type = FieldType.Tag)]
    public IEnumerable<string> WithNetworks { get; set; } = [];

    [FieldDefinition(11, Label = "ImportListsTmdbSettingsIncludeNullFirstAirDates", HelpText = "ImportListsTmdbSettingsIncludeNullFirstAirDatesHelpText", Type = FieldType.Checkbox, Advanced = true)]
    public bool IncludeNullFirstAirDates { get; set; }
}
