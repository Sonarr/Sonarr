using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListSettingsValidator : TraktSettingsBaseValidator<TraktListSettings>
    {
        public TraktListSettingsValidator()
        : base()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Listname).NotEmpty();
        }
    }

    public class TraktListSettings : TraktSettingsBase<TraktListSettings>
    {
        protected override AbstractValidator<TraktListSettings> Validator => new TraktListSettingsValidator();

        public TraktListSettings()
        {
        }

        [FieldDefinition(1, Label = "Username", HelpText = "Username for the List to import from")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "List Name", HelpText = "List name for import, list must be public or you must have access to the list")]
        public string Listname { get; set; }
    }
}
