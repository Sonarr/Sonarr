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

    public class ImdbListSettings : IImportListSettings
    {
        private static readonly ImdbSettingsValidator Validator = new ImdbSettingsValidator();

        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "ImportListsImdbSettingsListId", HelpText = "ImportListsImdbSettingsListIdHelpText")]
        public string ListId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
