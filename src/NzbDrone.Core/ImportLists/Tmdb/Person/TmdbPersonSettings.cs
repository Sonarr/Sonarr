using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonSettingsValidator : TmdbSettingsBaseValidator<TmdbPersonSettings>
{
    public TmdbPersonSettingsValidator()
    {
        RuleFor(c => c.PersonId).Must(id => int.TryParse(id, out var idInt) && idInt > 0)
            .WithMessage($"Must be a valid 32-bit integer greater than zero, and less than or equal to {int.MaxValue}.");

        RuleFor(c => c.IncludingCastCredits).Equal(true)
            .When(c => !c.IncludeDepartmentTypes.Any())
            .WithMessage("Must choose to include cast credit filter when not including crew department credits.");

        RuleFor(c => c.IncludeDepartmentTypes).NotEmpty()
            .When(c => !c.IncludingCastCredits)
            .WithMessage("Must choose at least one crew department credit filter when not including cast credit.");
    }
}

public class TmdbPersonSettings : TmdbSettingsBase<TmdbPersonSettings>
{
    private static readonly TmdbPersonSettingsValidator Validator = new();

    public TmdbPersonSettings()
        : base(Validator)
    {
        IncludingCastCredits = true;
        IncludeDepartmentTypes = [];
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsPersonId", HelpText = "ImportListsTmdbSettingsPersonIdHelpText", Type = FieldType.Textbox)]
    public string PersonId { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsIncludingCastCredits", HelpText = "ImportListsTmdbSettingsIncludingCastCreditsHelpText", Type = FieldType.Checkbox)]
    public bool IncludingCastCredits { get; set; }

    [FieldDefinition(3, Label = "ImportListsTmdbSettingsIncludeDepartmentTypes", HelpText = "ImportListsTmdbSettingsIncludeDepartmentTypesHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbDepartmentType))]
    public IEnumerable<int> IncludeDepartmentTypes { get; set; }
}
