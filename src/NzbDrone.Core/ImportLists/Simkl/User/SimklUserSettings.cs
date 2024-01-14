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

        [FieldDefinition(1, Label = "ImportListsSimklSettingsListType", Type = FieldType.Select, SelectOptions = typeof(SimklUserListType), HelpText = "ImportListsSimklSettingsListTypeHelpText")]
        public int ListType { get; set; }

        [FieldDefinition(1, Label = "ImportListsSimklSettingsShowType", Type = FieldType.Select, SelectOptions = typeof(SimklUserShowType), HelpText = "ImportListsSimklSettingsShowTypeHelpText")]
        public int ShowType { get; set; }
    }
}
