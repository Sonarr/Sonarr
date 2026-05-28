using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Person;

public class TMDbPersonSettingsValidator : TMDbSettingsBaseValidator<TMDbPersonSettings>
{
    public TMDbPersonSettingsValidator()
    {
        RuleFor(c => c.PersonId).Must(id => int.TryParse(id, out var idInt) && idInt > 0)
            .WithMessage($"Must be a valid 32-bit integer greater than zero, and less than or equal to {int.MaxValue}.");

        RuleFor(c => c.IsIncludingCastCredit).Equal(true)
            .When(c => !c.IncludedCrewDepartmentCredits.Any())
            .WithMessage("Must choose to include cast credit filter when not including crew department credits.");

        RuleFor(c => c.IncludedCrewDepartmentCredits).NotEmpty()
            .When(c => !c.IsIncludingCastCredit)
            .WithMessage("Must choose at least one crew department credit filter when not including cast credit.");
    }
}

public class TMDbPersonSettings : TMDbSettingsBase<TMDbPersonSettings>
{
    private static readonly TMDbPersonSettingsValidator Validator = new();

    public TMDbPersonSettings()
        : base(Validator)
    {
        IsIncludingCastCredit = true;
        IncludedCrewDepartmentCredits = [];
    }

    [FieldDefinition(1, Label = "ImportListsTMDbSettingsPersonId", HelpText = "ImportListsTMDbSettingsPersonIdHelpText", Type = FieldType.Textbox)]
    public string PersonId { get; set; }

    [FieldDefinition(2, Label = "ImportListsTMDbSettingsIsIncludingCastCredit", HelpText = "ImportListsTMDbSettingsIsIncludingCastCreditHelpText", Type = FieldType.Checkbox)]
    public bool IsIncludingCastCredit { get; set; }

    [FieldDefinition(3, Label = "ImportListsTMDbSettingsIncludedCrewDepartmentCredits", HelpText = "ImportListsTMDbSettingsIncludedCrewDepartmentCreditsHelpText", Type = FieldType.Select, SelectOptions = typeof(TMDbCrewDepartment))]
    public IEnumerable<int> IncludedCrewDepartmentCredits { get; set; }
}
