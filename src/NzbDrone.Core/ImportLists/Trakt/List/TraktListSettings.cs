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

        [FieldDefinition(1, Label = "Username", HelpText = "ImportListsTraktSettingsUsernameHelpText")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "ImportListsTraktSettingsListName", HelpText = "ImportListsTraktSettingsListNameHelpText")]
        public string Listname { get; set; }
    }
}
