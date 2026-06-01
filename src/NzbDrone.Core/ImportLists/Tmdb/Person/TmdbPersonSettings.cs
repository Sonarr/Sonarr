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

        RuleFor(c => c.IncludingCastCredit).Equal(true)
            .When(c => !c.IncludedCrewDepartmentCreditTypes.Any())
            .WithMessage("Must choose to include cast credit filter when not including crew department credits.");

        RuleFor(c => c.IncludedCrewDepartmentCreditTypes).NotEmpty()
            .When(c => !c.IncludingCastCredit)
            .WithMessage("Must choose at least one crew department credit filter when not including cast credit.");
    }
}

public class TmdbPersonSettings : TmdbSettingsBase<TmdbPersonSettings>
{
    private static readonly TmdbPersonSettingsValidator Validator = new();

    public TmdbPersonSettings()
        : base(Validator)
    {
        IncludingCastCredit = true;
        IncludedCrewDepartmentCreditTypes = [];
    }

    [FieldDefinition(1, Label = "ImportListsTmdbSettingsPersonId", HelpText = "ImportListsTmdbSettingsPersonIdHelpText", Type = FieldType.Textbox)]
    public string PersonId { get; set; }

    [FieldDefinition(2, Label = "ImportListsTmdbSettingsIncludingCastCredit", HelpText = "ImportListsTmdbSettingsIncludingCastCreditHelpText", Type = FieldType.Checkbox)]
    public bool IncludingCastCredit { get; set; }

    [FieldDefinition(3, Label = "ImportListsTmdbSettingsIncludedCrewDepartmentCreditTypes", HelpText = "ImportListsTmdbSettingsIncludedCrewDepartmentCreditTypesHelpText", Type = FieldType.Select, SelectOptions = typeof(TmdbCrewDepartmentType))]
    public IEnumerable<int> IncludedCrewDepartmentCreditTypes { get; set; }
}
