using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList;

public class TraktSmartListSettingsValidator : TraktSettingsBaseValidator<TraktSmartListSettings>
{
    public TraktSmartListSettingsValidator()
    {
        RuleFor(c => c.SmartListId).NotEmpty();

        RuleFor(c => c.SmartListId)
            .Matches(@"^[a-z0-9-]+$")
            .When(c => c.SmartListId.IsNotNullOrWhiteSpace())
            .WithMessage("Must be a Trakt smart list slug");
    }
}

public class TraktSmartListSettings : TraktSettingsBase<TraktSmartListSettings>
{
    private static readonly TraktSmartListSettingsValidator Validator = new();

    [FieldDefinition(1, Label = "ImportListsTraktSettingsSmartList", HelpText = "ImportListsTraktSettingsSmartListHelpText")]
    public string SmartListId { get; set; }

    public override NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(Validator.Validate(this));
    }
}
