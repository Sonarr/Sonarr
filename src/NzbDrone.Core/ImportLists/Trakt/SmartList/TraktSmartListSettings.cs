using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public class TraktSmartListSettingsValidator : TraktSettingsBaseValidator<TraktSmartListSettings>
    {
        public TraktSmartListSettingsValidator()
        {
            RuleFor(c => c.SmartList).NotEmpty();

            RuleFor(c => c.SmartList)
                .Must(TraktSmartListSlug.IsValid)
                .When(c => c.SmartList.IsNotNullOrWhiteSpace())
                .WithMessage("Must be a Trakt smart list URL or slug");
        }
    }

    public class TraktSmartListSettings : TraktSettingsBase<TraktSmartListSettings>
    {
        private static readonly TraktSmartListSettingsValidator Validator = new();

        [FieldDefinition(1, Label = "ImportListsTraktSettingsSmartList", HelpText = "ImportListsTraktSettingsSmartListHelpText")]
        public string SmartList { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
