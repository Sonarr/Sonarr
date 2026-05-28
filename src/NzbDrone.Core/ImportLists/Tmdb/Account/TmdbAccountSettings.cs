using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountSettingsValidator : TmdbSettingsBaseValidator<TmdbAccountSettings>
{
}

public class TmdbAccountSettings : TmdbSettingsBase<TmdbAccountSettings>
{
    private static readonly TmdbAccountSettingsValidator Validator = new();

    public TmdbAccountSettings()
        : base(Validator)
    {
        AccountList = (int)TmdbAccountList.Watchlist;
    }

    [FieldDefinition(1, Label = "Account List Type", HelpText = "Select the list type of the account to import.", Type = FieldType.Select, SelectOptions = typeof(TmdbAccountList))]
    public int AccountList { get; set; }
}
