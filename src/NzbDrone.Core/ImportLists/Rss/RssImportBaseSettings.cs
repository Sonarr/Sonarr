using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportSettingsValidator : AbstractValidator<RssImportBaseSettings>
    {
        public RssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class RssImportBaseSettings : IImportListSettings
    {
        private RssImportSettingsValidator Validator => new RssImportSettingsValidator();

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Url", Type = FieldType.Textbox)]
        public virtual string Url { get; set; }

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
