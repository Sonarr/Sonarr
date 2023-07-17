using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    internal class MalSettingsValidator : AbstractValidator<MalListSettings>
    {
        public MalSettingsValidator()
        {
        }
    }

    internal class MalListSettings : IImportListSettings
    {
        public string BaseUrl { get; set; }

        protected AbstractValidator<MalListSettings> Validator => new MalSettingsValidator();

        // This constructor is called when we try to add a new list
        public MalListSettings()
        {
            BaseUrl = "";
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
