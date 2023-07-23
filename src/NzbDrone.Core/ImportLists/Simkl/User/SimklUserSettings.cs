using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserSettingsValidator : SimklSettingsBaseValidator<SimklUserSettings>
    {
        public SimklUserSettingsValidator()
        : base()
        {
            RuleFor(c => c.ListType).NotNull();
        }
    }

    public class SimklUserSettings : SimklSettingsBase<SimklUserSettings>
    {
        protected override AbstractValidator<SimklUserSettings> Validator => new SimklUserSettingsValidator();

        public SimklUserSettings()
        {
            ListType = (int)SimklUserListType.Watching;
            ShowType = (int)SimklUserShowType.Shows;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(SimklUserListType), HelpText = "Type of list you're seeking to import from")]
        public int ListType { get; set; }

        [FieldDefinition(1, Label = "Show Type", Type = FieldType.Select, SelectOptions = typeof(SimklUserShowType), HelpText = "Type of show you're seeking to import from")]
        public int ShowType { get; set; }
    }
}
