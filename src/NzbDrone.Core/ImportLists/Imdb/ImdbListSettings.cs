using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Imdb
{
    public class ImdbSettingsValidator : AbstractValidator<ImdbListSettings>
    {
        public ImdbSettingsValidator()
        {
            RuleFor(c => c.ListId)
                .Matches(@"^ls\d+$")
                .WithMessage("List ID mist be an IMDb List ID of the form 'ls12345678'");
        }
    }

    public class ImdbListSettings : ImportListSettingsBase<ImdbListSettings>
    {
        private static readonly ImdbSettingsValidator Validator = new ();

        public override string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "ImportListsImdbSettingsListId", HelpText = "ImportListsImdbSettingsListIdHelpText")]
        public string ListId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
