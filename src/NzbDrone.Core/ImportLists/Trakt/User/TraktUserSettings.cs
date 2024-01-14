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
            TraktWatchSorting = (int)TraktUserWatchSorting.Rank;
        }

        [FieldDefinition(1, Label = "ImportListsTraktSettingsListType", Type = FieldType.Select, SelectOptions = typeof(TraktUserListType), HelpText = "ImportListsTraktSettingsListTypeHelpText")]
        public int TraktListType { get; set; }

        [FieldDefinition(2, Label = "ImportListsTraktSettingsWatchedListFilter", Type = FieldType.Select, SelectOptions = typeof(TraktUserWatchedListType), HelpText = "ImportListsTraktSettingsWatchedListFilterHelpText")]
        public int TraktWatchedListType { get; set; }

        [FieldDefinition(3, Label = "ImportListsTraktSettingsWatchedListSorting", Type = FieldType.Select, SelectOptions = typeof(TraktUserWatchSorting), HelpText = "ImportListsTraktSettingsWatchedListSortingHelpText")]
        public int TraktWatchSorting { get; set; }

        [FieldDefinition(4, Label = "Username", HelpText = "ImportListsTraktSettingsUserListUsernameHelpText")]
        public string Username { get; set; }
    }

    public enum TraktUserWatchSorting
    {
        Rank = 0,
        Added = 1,
        Title = 2,
        Released = 3
    }
}
