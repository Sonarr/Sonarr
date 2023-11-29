using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserSettingsValidator : TraktSettingsBaseValidator<TraktUserSettings>
    {
        public TraktUserSettingsValidator()
        : base()
        {
            RuleFor(c => c.TraktListType).NotNull();
            RuleFor(c => c.TraktWatchedListType).NotNull();
            RuleFor(c => c.AuthUser).NotEmpty();
        }
    }

    public class TraktUserSettings : TraktSettingsBase<TraktUserSettings>
    {
        protected override AbstractValidator<TraktUserSettings> Validator => new TraktUserSettingsValidator();

        public TraktUserSettings()
        {
            TraktListType = (int)TraktUserListType.UserWatchList;
            TraktWatchedListType = (int)TraktUserWatchedListType.All;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktUserListType), HelpText = "Type of list you're seeking to import from")]
        public int TraktListType { get; set; }

        [FieldDefinition(2, Label = "Watched List Filter", Type = FieldType.Select, SelectOptions = typeof(TraktUserWatchedListType), HelpText = "If List Type is Watched. Series do you want to import from")]
        public int TraktWatchedListType { get; set; }

        [FieldDefinition(3, Label = "Username", HelpText = "Username for the List to import from (empty to use Auth User)")]
        public string Username { get; set; }
    }
}
